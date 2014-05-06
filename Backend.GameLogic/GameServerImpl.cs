namespace Backend.GameLogic
{
    using Backend.Utils;
    using Messages;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class GameServerImpl : ITcpServerHandler
    {
        private Dictionary<int, Queue<GameServerMessageBase>> queues = new Dictionary<int, Queue<GameServerMessageBase>>();

        async Task ITcpServerHandler.HandleRequest(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            try
            {
                Socket client = tcpClient.Client;

                var joinMessageResponse = await client.ReadCommandOrErrorAsync<JoinGameMessage>();
                if (joinMessageResponse.IsError)
                {
                    await client.WriteCommandAsync(new ErrorMessage(string.Format("Sorry, was expecting a {0}", typeof(JoinGameMessage).Name)));
                    return;
                }
                var joinMessage = joinMessageResponse.Message;
                var myQueue = new Queue<GameServerMessageBase>();
                this.queues.Add(joinMessage.ClientID.ID, myQueue);

                var receiveTask = Task.Factory.StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        SomeGameMessage someGameMessage = await client.ReadExpectedCommandAsync<SomeGameMessage>();
                        Console.WriteLine("Received message {0} from {1}", someGameMessage.Stuff, joinMessage.ClientID.ID);

                        foreach (var queue in this.queues.Values)
                        {
                            if (queue != myQueue)
                            {
                                queue.Enqueue(someGameMessage);
                            }
                        }
                    }
                }).Unwrap();

                var senderTask = Task.Factory.StartNew(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (myQueue.Count > 0)
                        {
                            var messageToSent = myQueue.Dequeue();
                            await client.WriteCommandAsync(messageToSent);
                            Console.WriteLine("Sent message {0} to client {1}", ((SomeGameMessage)messageToSent).Stuff, joinMessage.ClientID.ID);
                        }
                    }
                }).Unwrap();

                await Task.WhenAll(receiveTask, senderTask);
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format("{0}: {1}", ex.GetType().Name, ex.Message));
            }
        }
    }
}
