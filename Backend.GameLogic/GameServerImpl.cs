namespace Backend.GameLogic
{
    using Backend.GameLogic.Security;
    using Backend.Utils;
    using Messages;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class GameServerImpl : ITcpServerHandler
    {
        public PlayerAuthenticator PlayerAuthenticator { get; private set; }

        public GameServerImpl(byte[] secretKey)
        {
            this.PlayerAuthenticator = new PlayerAuthenticator(secretKey);
        }

        private Dictionary<int, ConcurrentQueue<GameServerMessageBase>> queues = new Dictionary<int, ConcurrentQueue<GameServerMessageBase>>();

        async Task ITcpServerHandler.HandleRequest(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            try
            {
                Socket client = tcpClient.Client;

                var joinMessageResponse = await client.ReadCommandOrErrorAsync<LoginToGameServerRequest>();
                if (joinMessageResponse.IsError)
                {
                    await client.WriteCommandAsync(new ErrorMessage(string.Format("Sorry, was expecting a {0}", typeof(LoginToGameServerRequest).Name)));
                    return;
                }
                var joinMessage = joinMessageResponse.Message;

                var gameserverId = "gameserver123";
                var clientId = this.PlayerAuthenticator.ValidateClientID(joinMessage.Token, gameserverId);

                var myQueue = new ConcurrentQueue<GameServerMessageBase>();
                this.queues.Add(clientId.ID, myQueue);

                Task receiveTask = Task.Factory.StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        SomeGameMessage someGameMessage = await client.ReadExpectedCommandAsync<SomeGameMessage>();
                        someGameMessage.From = clientId;

                        Console.WriteLine("Received message {0} from {1}", someGameMessage.Stuff, someGameMessage.From.ID);

                        foreach (var queue in this.queues.Values)
                        {
                            if (queue != myQueue)
                            {
                                queue.Enqueue(someGameMessage);
                            }
                        }
                    }
                }).Unwrap();

                Task senderTask = Task.Factory.StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (myQueue.Count > 0)
                        {
                            GameServerMessageBase messageToSent;

                            if (myQueue.TryDequeue(out messageToSent))
                            {
                                await client.WriteCommandAsync(messageToSent);
                                Console.WriteLine("Sent message {0} to client {1}", ((SomeGameMessage)messageToSent).Stuff, clientId.ID);
                            }
                        }
                    }
                }).Unwrap();

                await Task.WhenAll(receiveTask, senderTask);

                this.queues.Remove(clientId.ID);
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
        }
    }
}
