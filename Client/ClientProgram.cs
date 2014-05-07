namespace Client
{
    using Backend.GameLogic;
    using Backend.GameLogic.Messages;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    class ClientProgram
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";

            int clientCount = 1;

            while (true)
            {
                Console.Write("Enter client ID: ");
                var s = Console.ReadLine();
                var clientId = int.Parse(s);

                var clientTasks = Task.Factory.StartNew(async () => 
                {
                    var p = new ClientProgram();
                    await p.RunGameClientAsync(new ClientID { ID = clientId });
                }).Unwrap();

                Task.WaitAll(clientTasks);
            }

            //var p = new ClientProgram();
            //p.RunAsync().Wait();
        }

        private static async Task Log(string s)
        {
            // Console.WriteLine(s);
        }

        public async Task<GameServerConnectionMessage> RunLobbyclientAsync(ClientID clientId)
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

            // Console.WriteLine("I should connect to {0} using credential {1}", gameServerInfo.GameServer.ToString(), gameServerInfo.Token.Credential);

            return gameServerInfo;
        }

        public async Task RunGameClientAsync(ClientID clientId)
        {
            var client = new TcpClient();

            await client.ConnectAsync(address: IPAddress.Parse("127.0.0.1"), port: 4000);

            var server = client.Client;

            await server.WriteCommandAsync(new JoinGameMessage { ClientID = clientId });

            var receiveTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var someGameMessage = await server.ReadExpectedCommandAsync<SomeGameMessage>();
                    Console.WriteLine("Received \"{0}\" from {1}", someGameMessage.Stuff, someGameMessage.From.ID);
                }
            }).Unwrap();

            var senderTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var content = Console.ReadLine();
                    await server.WriteCommandAsync(new SomeGameMessage { Stuff = content, From = new ClientID { ID = -1 } });
                    Console.WriteLine("Sent message");
                }
            }).Unwrap();

            await Task.WhenAll(receiveTask, senderTask);

            client.Close();
        }
    }
}