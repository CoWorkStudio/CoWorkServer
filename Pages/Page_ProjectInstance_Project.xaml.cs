﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Co_work.Windows;
using FTPHelper;

namespace Co_work.Pages
{
    /// <summary>
    /// Page_ProjectInstance_Project.xaml 的交互逻辑
    /// </summary>
    public partial class Page_ProjectInstance_Project : Page
    {
        public Page_ProjectInstance_Project()
        {
            InitializeComponent();
            Lv_File.ContextMenu = MenuListView();
        }

        public Page_ProjectInstance Owner;

        public FtpHelper ftpHelper = new FtpHelper("103.193.189.241", "Administrator", "adxq@9139");

        class FileItem
        { 
            public string name { get; set;}
            public string uper { get; set; }
            public string date { get; set; }
            public string size { get; set; }
        }

        private void Btn_Upload_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                string fileAddress = dialog.FileName;
                string fileName = System.IO.Path.GetFileName(fileAddress);

                FileUploadAsync(fileAddress, fileName);
            }
        }

        private async Task FileUploadAsync(string fileAddress, string fileName)
        {
            await Task.Run(() => FileUploadPlusAsync(fileAddress, fileName));
            RefreshListView();
        }

        public float persent;

        private void FileUploadPlusAsync(string fileAddress, string fileName)
        {
            Task.Run(() => FileUpload(fileAddress, fileName, ref persent));
            while (true)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    Pb_test.Value = persent;
                }));

                if (persent == 100)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Pb_test.Value = 100;
                    }));
                    break;
                }
            }
        }

        private void FileUpload(string fileAddress, string fileName, ref float persent)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileAddress);
            ftpHelper.Upload(fileInfo, "/Test/" + fileName, ref persent);
        }

        private void FileItemCreate(string fileName , string fileSize)
        {
            FileItem file = new FileItem { name = fileName , size = fileSize};
            ListViewItem item = new ListViewItem();
            item.Content = file;
            item.DataContext = file;
            Lv_File.Items.Add(item);
            item.ContextMenu = MenuFile();
        }


        void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }


        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            Window_RenameFile window = new Window_RenameFile();
            window.Owner1 = this;
            window.Lb_FileName.Text = ((Lv_File.SelectedItem as ListViewItem).DataContext as FileItem).name;
            window.ShowDialog();
        }

        public async Task Rename(string newFileName)
        {
            await Task.Run(() =>
            Dispatcher.Invoke(new Action(delegate
            {
                ftpHelper.Rename("/Test/" + ((Lv_File.SelectedItem as ListViewItem).DataContext as FileItem).name, newFileName);
            })));
            RefreshListView();
        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            DeleteFile();
        }

        private async Task DeleteFile()
        {
            await Task.Run(() =>
            Dispatcher.Invoke(new Action(delegate
            {
                ftpHelper.Delete("/Test/" + ((Lv_File.SelectedItem as ListViewItem).DataContext as FileItem).name);
            })));
            RefreshListView();
        }


        private void DeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            DeleteFolder();
        }

        private async Task DeleteFolder()
        {
            await Task.Run(() =>
                Dispatcher.Invoke(new Action(delegate
                {
                    ftpHelper.DeleteDir("/Test/" + ((Lv_File.SelectedItem as ListViewItem).DataContext as FileItem).name);
                })));
            RefreshListView();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshListView();
        }

        public void RefreshListView()
        {
            Task.Run(() => RefreshListViewAsync());
        }

        private async Task RefreshListViewAsync()
        {
            List<string> listFolder = ftpHelper.GetDirctory("/Test/");
            List<string> listFile = ftpHelper.GetFile("/Test/");

            Dispatcher.Invoke(new Action(delegate
            {
                Lv_File.Items.Clear();
            }));
            
            foreach (var itemFolder in listFolder)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    CreateNewFolderItem(itemFolder);
                }));
            }
            foreach (var itemFile in listFile)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    FileItemCreate(itemFile, ftpHelper.GetFileSize("/Test/" + itemFile).ToString());
                }));
            }
        }


        private void Btn_NewFolder_Click(object sender, RoutedEventArgs e)
        {
            Window_NewFolder window = new Window_NewFolder();
            window.Owner1 = this;
            window.ShowDialog();
        }

        public void CreateNewFolder(string newFolderName)
        {
            ftpHelper.CreateDir("/Test/" + newFolderName);
            RefreshListView();
        }

        public void CreateNewFolderItem(string newFolderName)
        {
            FileItem file = new FileItem { name = newFolderName };
            ListViewItem item = new ListViewItem();
            item.Content = file;
            Lv_File.Items.Add(item);
            item.MouseDoubleClick += ListViewItem_MouseDoubleClick;
            item.DataContext = file;
            item.ContextMenu = MenuFolder();
        }

        private ContextMenu MenuFile()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem itemRename = new MenuItem { Name = "Rename", Header = "重命名" };
            itemRename.Click += Rename_Click;
            MenuItem itemDelete = new MenuItem { Name = "Delete", Header = "删除文件" };
            itemDelete.Click += DeleteFile_Click;
            MenuItem itemDownload = new MenuItem { Name = "Download", Header = "下载文件" };
            itemDownload.Click += Download_Click;
            menu.Items.Add(itemRename);
            menu.Items.Add(itemDelete);
            menu.Items.Add(itemDownload);

            return menu;
        }

        private ContextMenu MenuFolder()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem itemRename = new MenuItem { Name = "Rename", Header = "重命名" };
            itemRename.Click += Rename_Click;
            MenuItem itemDelete = new MenuItem { Name = "Delete", Header = "删除文件夹" };
            itemDelete.Click += DeleteFolder_Click;
            menu.Items.Add(itemRename);
            menu.Items.Add(itemDelete);

            return menu;
        }

        private ContextMenu MenuListView()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem itemRefresh = new MenuItem { Name = "Refresh", Header = "刷新" };
            itemRefresh.Click += Refresh_Click;
            MenuItem itemUpload = new MenuItem { Name = "Upload", Header = "上传文件" };
            itemUpload.Click += Btn_Upload_Click;
            MenuItem itemNewFolder = new MenuItem { Name = "NewFolder", Header = "新建文件夹" };
            itemNewFolder.Click += Btn_NewFolder_Click;
            menu.Items.Add(itemRefresh);
            menu.Items.Add(itemUpload);
            menu.Items.Add(itemNewFolder);

            return menu;
        }

    }
}
