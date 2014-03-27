namespace Backend.Utils.Networking.Extensions
{
    using System;
    using System.Reactive.Linq;
    using Microsoft.ServiceBus.Messaging;

    public static class ObservableServiceBusExtensions
    {
        public static IObservable<BrokeredMessage> CreateObervable(this SubscriptionClient client)
        {
            return Observable.Create<BrokeredMessage>(async (observer) =>
            {
                try
                {
                    while (!client.IsClosed)
                    {
                        var message = await client.ReceiveAsync();
                        if (message == null)
                        {
                            observer.OnCompleted();
                            break;
                        }

                        observer.OnNext(message);
                    }
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            });
        }

        public static IObservable<BrokeredMessage> CreateObervableBatch(this SubscriptionClient client, int messageCount)
        {
            return Observable.Create<BrokeredMessage>(async (observer) =>
            {
                try
                {
                    while (!client.IsClosed)
                    {
                        var messages = await client.ReceiveBatchAsync(messageCount);
                        if (messages == null)
                        {
                            observer.OnCompleted();
                            break;
                        }

                        foreach (var message in messages)
                        {
                            observer.OnNext(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            });
        }
    }
}
