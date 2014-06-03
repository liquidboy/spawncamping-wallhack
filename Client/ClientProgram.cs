namespace Client
{
    using Backend.GameLogic;
    using Backend.GameLogic.Messages;
    using Backend.GameLogic.Security;
    using Backend.GameLogic.Models;
    using Backend.Utils.Networking.Extensions;
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

            if (clientCount == 1)
            {
                var clientId = new ClientID { ID = Guid.NewGuid() };

                var gameserver = GetGameServerAsync(clientId, CompletelyInsecureLobbyAuthentication.CreatePassword(clientId)).Result;

                Func<Task<string>> readline = () => 
                { 
                    Console.Write("Enter message: "); 
                    return Task.FromResult<string>(Console.ReadLine()); 
                };

                PlayGameAsync(clientId, gameserver, readline).Wait();
            }
            else if (clientCount > 1)
            {

                var clientTasks = Enumerable
                    .Range(1, clientCount)
                    .Select(_ => new ClientID { ID = Guid.NewGuid() })
                    .Select(clientId => new { ClientID = clientId, Password = CompletelyInsecureLobbyAuthentication.CreatePassword(clientId) })
                    .Select(_ => Task.Factory.StartNew(async () =>
                        {
                            var gameserver = await GetGameServerAsync(_.ClientID, _.Password);

                            int ctr = 1;
                            Func<Task<string>> iterate = async () =>
                            {
                                await Task.Delay(TimeSpan.FromSeconds(2));

                                return (ctr++).ToString();
                            };

                            await PlayGameAsync(_.ClientID, gameserver, iterate);
                        }).Unwrap())
                    .ToArray();

                Task.WaitAll(clientTasks);
            }
        }

        private static async Task Log(string s)
        {
            // Console.WriteLine(s);

            await Task.Yield();
        }

        public static async Task<LoginToLobbyResponseMessage> GetGameServerAsync(ClientID clientId, string password)
        {
            Console.WriteLine("Connect to lobby");

            var lobbyClient = new LobbyClientImpl(ipAddress: IPAddress.Loopback, port: 3000)
            {
                Logger = Log
            };

            await lobbyClient.ConnectAsync();

            Console.WriteLine("Connected to lobby");

            var gameServerInfo = await lobbyClient.JoinLobbyAsync(clientId, password);

            Console.WriteLine("Lobby interaction done: {0}", gameServerInfo);

            /*if (clientId % 10 == 0)*/
            Console.WriteLine("{0}: {1}", clientId, gameServerInfo.Token.Credential);

            lobbyClient.Close();

            // Console.WriteLine("I should connect to {0} using credential {1}", gameServerInfo.GameServer.ToString(), gameServerInfo.Token.Credential);

            return gameServerInfo;
        }

        public static async Task PlayGameAsync(ClientID clientId, LoginToLobbyResponseMessage gameserver, Func<Task<string>> GetChatMessage)
        {

            #region Establish TCP connection

            var client = new TcpClient();
            await client.ConnectAsync(
                address: gameserver.GameServer.Address, 
                port: gameserver.GameServer.Port);

            var server = client.Client;

            #endregion

            #region Send the port number to the proxy. 

            await server.WriteAsync(gameserver.InnergameServerPort);

            #endregion

            await server.WriteCommandAsync(new LoginToGameServerRequest(gameserver.Token));

            var receiveTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var someGameMessage = await server.ReadCommandOrErrorAsync<SomeGameMessage>();
                    if (someGameMessage.IsError)
                    {
                        Console.WriteLine("Error {0}", someGameMessage.ErrorMessage);
                    }
                    else
                    {
                        Console.WriteLine("Received \"{0}\" from {1}", someGameMessage.Message.Stuff, someGameMessage.Message.From.ID);
                    }
                }
            }).Unwrap();

            var senderTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        var content = await GetChatMessage();
                        await server.WriteCommandAsync(new SomeGameMessage { Stuff = content, From = new ClientID { ID = Guid.Empty } });
                        Console.WriteLine("Client {0} sending message {1}", clientId.ID, content);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                }
            }).Unwrap();

            await Task.WhenAll(receiveTask, senderTask);

            client.Close();
        }
    }
}