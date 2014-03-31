namespace Backend.Utils.Networking.Extensions
{
    using System;
    using System.Reactive.Linq;
    using Microsoft.ServiceBus.Messaging;
    using System.Threading.Tasks;
    using System.Threading;


    public interface IBusMessage<T>
    {
        T Content { get; }

        object this[string key] { get; }
    }

    public class ServiceBusBusMessage<T> : IBusMessage<T>
    {
        public ServiceBusBusMessage(BrokeredMessage brokeredMessage)
        {
            this.BrokeredMessage = brokeredMessage;
        }

        public BrokeredMessage BrokeredMessage { get; private set; }

        private readonly object m_lock = new object();
        private T m_t;
        T IBusMessage<T>.Content
        {
            get 
            {
                if (m_t == null)
                {
                    lock (m_lock)
                    {
                        if (m_t == null)
                        {
                            m_t = this.BrokeredMessage.GetBody<T>();
                        }
                    }
                }

                return m_t;
            }
        }

        object IBusMessage<T>.this[string key]
        {
            get 
            {
                if (!this.BrokeredMessage.Properties.Keys.Contains(key))
                { 
                    return null; 
                }
                
                return this.BrokeredMessage.Properties[key];
            }
        }
    }

    public static class ObservableServiceBusExtensions
    {
        public static IObservable<IBusMessage<T>> CreateObervable<T>(this SubscriptionClient client)
        {
            return CreateObervable<T>(client, CancellationToken.None);

        }

        public static IObservable<IBusMessage<T>> CreateObervable<T>(this SubscriptionClient client, CancellationToken cancellationToken)
        {
            Func<IObserver<ServiceBusBusMessage<T>>, Task> t = async (observer) =>
            {
                try
                {
                    while (!client.IsClosed)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var message = await client.ReceiveAsync(serverWaitTime: TimeSpan.FromSeconds(10));
                        if (message == null)
                        {
                            continue;
                        }

                        var body = new ServiceBusBusMessage<T>(message);

                        observer.OnNext(body);
                    }

                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            };

            return Observable
                .Create<ServiceBusBusMessage<T>>(t)
                .Publish()
                .RefCount();
        }


        public static IObservable<IBusMessage<T>> CreateObervableBatch<T>(this SubscriptionClient client, int messageCount)
        {
            return CreateObervableBatch<T>(client, messageCount, CancellationToken.None);
        }

        public static IObservable<IBusMessage<T>> CreateObervableBatch<T>(this SubscriptionClient client, int messageCount, CancellationToken cancellationToken)
        {
            Func<IObserver<ServiceBusBusMessage<T>>, Task> t = async (observer) =>  {
                try
                {
                    while (!client.IsClosed)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var messages = await client.ReceiveBatchAsync(messageCount);
                        if (messages == null)
                        {
                            observer.OnCompleted();
                            break;
                        }

                        foreach (var message in messages)
                        {
                            observer.OnNext(new ServiceBusBusMessage<T>(message));
                        }
                    }
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            };

            return Observable
              .Create<ServiceBusBusMessage<T>>(t)
              .Publish()
              .RefCount();
        }
    }
}
