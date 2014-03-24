namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Backend.Utils.Networking.Extensions;

    public class LobbyServerImpl
    {
        public LobbyServerImpl() { }

        public async Task HandleClient(TcpClient tcpClient, CancellationToken ct)
        {
            var connection = new LobbyConnection(tcpClient);

            await connection.Handlerequest();
        }
    }

    public class LobbyConnection
    {
        private readonly TcpClient tcpClient;

        public LobbyConnection(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }

        public async Task Handlerequest()
        {
            Socket socket = tcpClient.Client;

            var clientId = await JoinLobbyAsync(socket);
            Console.WriteLine("Connect from client {0}", clientId);
        }

        public async Task<int> JoinLobbyAsync(Socket socket)
        {
            var clientId = await socket.ReadInt32Async();

            return clientId;
        }
    }
}
