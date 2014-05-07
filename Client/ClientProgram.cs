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

            Console.Write("Enter client count: ");
            var clientCount = int.Parse(Console.ReadLine());

            var clientTasks = Enumerable
                .Range(1, clientCount)
                .Select(_ => new ClientID { ID = _ })
                .Select(clientID => new { ClientID = clientID, Password = CompletelyInsecureLobbyAuthentication.CreatePassword(clientID) }) 
                .Select(_ => Task.Factory.StartNew(async () => 
                    {
                        var gameserver = await GetGameServerAsync(_.ClientID, _.Password);

                        await PlayGameAsync(gameserver);
                    }).Unwrap())
                .ToArray();

            Task.WaitAll(clientTasks);
        }

        private static async Task Log(string s)
        {
            // Console.WriteLine(s);
        }

        public static async Task<LoginToLobbyResponseMessage> GetGameServerAsync(ClientID clientId, string password)
        {
            var lobbyClient = new LobbyClientImpl(ipAddress: IPAddress.Loopback, port: 3000)
            {
                Logger = Log
            };

            await lobbyClient.ConnectAsync();

            var gameServerInfo = await lobbyClient.JoinLobbyAsync(clientId, password);

            /*if (clientId % 10 == 0)*/
            Console.WriteLine("{0}: {1}", clientId, gameServerInfo.Token.Credential);

            lobbyClient.Close();

            // Console.WriteLine("I should connect to {0} using credential {1}", gameServerInfo.GameServer.ToString(), gameServerInfo.Token.Credential);

            return gameServerInfo;
        }

        public static async Task PlayGameAsync(LoginToLobbyResponseMessage gameserver)
        {

            var client = new TcpClient();
            await client.ConnectAsync(
                address: gameserver.GameServer.Address, 
                port: gameserver.GameServer.Port);

            var server = client.Client;

            await server.WriteCommandAsync(new LoginToGameServerRequest(gameserver.Token));

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