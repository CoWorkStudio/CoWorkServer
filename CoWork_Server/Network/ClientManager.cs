using System.Collections.Generic;
using Co_Work.Network.TCP;

namespace Co_Work.Network
{
    public static class ClientManager
    {
        private static Dictionary<string, Client> ClientLists { get; } = new();

        public static void AddClient(string clientGuid,Client client)
        {
            ClientLists.Add(clientGuid,client);
        }

        public static void RemoveClient(string clientGuid)
        {
            ClientLists.Remove(clientGuid);
        }

        public static Client GetClient(string clientGuid)
        {
            return ClientLists[clientGuid];
        }
    }
}