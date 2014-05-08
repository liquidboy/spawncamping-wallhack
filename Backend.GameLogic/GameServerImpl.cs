namespace Backend.GameLogic
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Backend.Utils;
    using Messages;
    using Security;
    using Models;

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
            CancellationToken replacementToken;
            CancellationTokenSource cancellationTokenSource = cancellationToken.DerivedToken(out replacementToken);
            cancellationToken = replacementToken;

            Socket client = tcpClient.Client;
            ClientID clientId = null;

            try
            {
                Trace.TraceInformation("New client connection coming in...");

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
                bool clientExistsAlready = false;
                this.queues.AddOrUpdate(clientId.ID, myQueue, (k, v) => { clientExistsAlready = true; return v; });
                if (clientExistsAlready)
                {
                    await client.WriteCommandAsync(new ErrorMessage(string.Format("Sorry, you already have some other connection going")));
                    return;
                }

                Task receiveTask = Task.Factory.StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        SomeGameMessage someGameMessage = await client.ReadExpectedCommandAsync<SomeGameMessage>();
                        someGameMessage.From = clientId;

                        Trace.TraceInformation(string.Format("Received message {0} from {1}", someGameMessage.Stuff, someGameMessage.From.ID));

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

                            Trace.TraceInformation(string.Format("Sent message {0} to client {1}", ((SomeGameMessage)messageToSent).Stuff, clientId.ID));
                        }
                    }
                }).Unwrap();

                await Task.WhenAny(receiveTask, senderTask);

                ConcurrentQueue<GameServerMessageBase> q;
                this.queues.TryRemove(clientId.ID, out q);

                Trace.TraceInformation(string.Format("queues.Count: {0}", this.queues.Count));
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
            finally
            {
                cancellationTokenSource.Cancel();
                client.Close();
            }
        }
    }
}