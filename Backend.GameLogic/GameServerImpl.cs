namespace Backend.GameLogic
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Backend.GameLogic.Security;
    using Backend.Utils;
    using Messages;

    public class GameServerImpl : ITcpServerHandler
    {
        public PlayerAuthenticator PlayerAuthenticator { get; private set; }

        public GameServerImpl(byte[] secretKey)
        {
            this.PlayerAuthenticator = new PlayerAuthenticator(secretKey);
        }


        private ConcurrentDictionary<int, ConcurrentQueue<GameServerMessageBase>> queues = 
            new ConcurrentDictionary<int, ConcurrentQueue<GameServerMessageBase>>();

        async Task ITcpServerHandler.HandleRequest(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            Socket client = tcpClient.Client;
            ClientID clientId = null;

            try
            {
                Console.WriteLine("New client connection coming in...");

                var joinMessageResponse = await client.ReadCommandOrErrorAsync<LoginToGameServerRequest>();
                if (joinMessageResponse.IsError)
                {
                    await client.WriteCommandAsync(new ErrorMessage(string.Format("Sorry, was expecting a {0}", typeof(LoginToGameServerRequest).Name)));
                    return;
                }
                var joinMessage = joinMessageResponse.Message;

                var gameserverId = "gameserver123";
                clientId = this.PlayerAuthenticator.ValidateClientID(joinMessage.Token, gameserverId);

                var myQueue = new ConcurrentQueue<GameServerMessageBase>();
                this.queues.AddOrUpdate(clientId.ID, myQueue, (k,v) => myQueue);

                Task receiveTask = Task.Factory.StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        SomeGameMessage someGameMessage = await client.ReadExpectedCommandAsync<SomeGameMessage>();
                        someGameMessage.From = clientId;

                        Console.WriteLine("Received message {0} from {1}", someGameMessage.Stuff, someGameMessage.From.ID);

                        foreach (var queue in this.queues)
                        {
                            if (queue.Key != clientId.ID && queue.Value != null)
                            {
                                queue.Value.Enqueue(someGameMessage);
                            }
                        }
                    }
                }).Unwrap();

                Task senderTask = Task.Factory.StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (myQueue.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(5));
                            continue;
                        }

                        GameServerMessageBase messageToSent;
                        if (myQueue.TryDequeue(out messageToSent))
                        {
                            await client.WriteCommandAsync(messageToSent);

                            Console.WriteLine("Sent message {0} to client {1}", ((SomeGameMessage)messageToSent).Stuff, clientId.ID);
                        }
                    }
                }).Unwrap();

                await Task.WhenAny(receiveTask, senderTask);

                Console.WriteLine("Task ended...");
                Console.WriteLine("receiveTask: {0}", receiveTask.Status);
                Console.WriteLine("senderTask: {0}", senderTask.Status);

                ConcurrentQueue<GameServerMessageBase> q;
                this.queues.TryRemove(clientId.ID, out q);

                Console.WriteLine("queues.Count: {0}", this.queues.Count);
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
            finally
            {
                if (clientId != null)
                {
                    Console.WriteLine("Closing connection on client {0}", clientId.ID);
                }
                else
                {
                    Console.WriteLine("Closing connection on unknown client");
                }
                client.Close();
            }
        }
    }
}
