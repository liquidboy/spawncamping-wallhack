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

            int clientCount = 10;

            while (true)
            {
                Console.Write("Enter number of clients to simulate (default is {0}): ", clientCount);
                var s = Console.ReadLine();
                var oldClientCount = clientCount;
                if (!int.TryParse(s, out clientCount)) { clientCount = oldClientCount; }

                var clientTasks = Enumerable.Range(1, clientCount).Select(async (clientId) =>
                {
                    await Task.Delay(clientId * 5);
                    var p = new ClientProgram();
                    await p.RunAsync(new ClientID { ID = clientId });
                }).ToArray();

                Task.WaitAll(clientTasks);
            }

            //var p = new ClientProgram();
            //p.RunAsync().Wait();
        }

        private static async Task Log(string s)
        {
            // Console.WriteLine(s);
        }

        public async Task RunAsync(ClientID clientId)
        {
            var lobbyClient = new LobbyClientImpl(ipAddress: IPAddress.Loopback, port: 3003)
            {
                Logger = Log
            };

            await lobbyClient.ConnectAsync();

            // Console.WriteLine("Connected");

            var gameServerInfo = await lobbyClient.JoinLobbyAsync(clientId);

            /*if (clientId % 10 == 0)*/
            Console.WriteLine("{0}: {1}", clientId, gameServerInfo.Token.Credential);

            lobbyClient.Close();
            Console.WriteLine();

            // Console.WriteLine("I should connect to {0} using credential {1}", gameServerInfo.GameServer.ToString(), gameServerInfo.Token.Credential);
        }
    }
}