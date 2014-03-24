namespace Backend.LobbyServer
{
    using Backend.GameLogic;
    using Backend.Utils.Networking;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    class LobbyServerProgram
    {
        static void Main(string[] args)
        {
            Console.Title = "Backend.LobbyServer";


            var cts = new CancellationTokenSource();
            var server = new AsyncServerHost(ipAddress: IPAddress.Loopback, port: 3000);

            var lobbyServerImpl = new LobbyServerImpl();
            Task t = server.Start(lobbyServerImpl.Server, cts.Token);

            Console.ReadLine();
            cts.Cancel();
        }
    }
}