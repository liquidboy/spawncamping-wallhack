namespace Client
{
    using Backend.GameLogic;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    class ClientProgram
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";

            Console.Write("Press <return> to connect");
            Console.ReadLine();

            var p = new ClientProgram();
            p.RunAsync().Wait();
        }

        private static async Task Log(string s)
        {
            Console.WriteLine(s);
        }

        public async Task RunAsync()
        {
            var lobbyClient = new LobbyClientImpl(ipAddress: IPAddress.Loopback, port: 3000)
            {
                Logger = Log
            };

            await lobbyClient.ConnectAsync();

            Console.WriteLine("Connected");

            var gameServerInfo = await lobbyClient.JoinLobbyAsync(clientId: 2000);

            Console.WriteLine("I should connect to {0} using credential {1}", gameServerInfo.GameServer.ToString(), gameServerInfo.Token.Credential);
        }
    }
}