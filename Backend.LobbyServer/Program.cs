namespace Backend.LobbyServer
{
    using Backend.GameLogic;
    using Backend.Utils.Networking;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Lobby Server";

            var lobbyServerImpl = new LobbyServerImpl();

            Console.Title = "Server";
            var cts = new CancellationTokenSource();
            var server = new AsyncServerHost(ipAddress: IPAddress.Loopback, port: 3000);
            Task t = server.Start(lobbyServerImpl.Server, cts.Token);

            Console.WriteLine("Launched Server");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}