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

            var clientTasks = Enumerable.Range(10, 50000).Select(async (clientId) =>
            {
                await Task.Delay(clientId * 5);
                var p = new ClientProgram();
                await p.RunAsync(clientId: clientId);
            }).ToArray();

            Task.WaitAll(clientTasks);

            //var p = new ClientProgram();
            //p.RunAsync().Wait();
        }

        private static async Task Log(string s)
        {
            // Console.WriteLine(s);
        }

        public async Task RunAsync(int clientId)
        {
            var lobbyClient = new LobbyClientImpl(ipAddress: IPAddress.Loopback, port: 3003)
            {
                Logger = Log
            };

            await lobbyClient.ConnectAsync();

            // Console.WriteLine("Connected");

            var gameServerInfo = await lobbyClient.JoinLobbyAsync(clientId);

            if (clientId % 10 == 0) Console.Write(".");

            lobbyClient.Close();

            // Console.WriteLine("I should connect to {0} using credential {1}", gameServerInfo.GameServer.ToString(), gameServerInfo.Token.Credential);
        }
    }
}