namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reactive.Linq;
    using System.ComponentModel.Composition;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Backend.Utils.Networking.Extensions;
    using System.Diagnostics;

    [Export(typeof(LobbyServiceBackplane))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LobbyServiceBackplane : IPartImportsSatisfiedNotification, IDisposable
    {
        [Import(typeof(ILobbyServiceSettings))]
        public ILobbyServiceSettings Settings { get; set; }

        public IObservable<BrokeredMessage> ObservableBackPlane { get; private set; }

        public string TopicPath { get { return "LobbyServiceTopic"; } }
        
        public string SubscriptionName { get { return this.Settings.LobbyServiceInstanceId; } }
        
        private NamespaceManager _namespaceManager;

        private TopicClient _TopicClient;

        private readonly int messagesToFetchAtOnce = 10;

        private SubscriptionClient _SubscriptionClient;

        public LobbyServiceBackplane() { }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() { this.OnImportsSatisfiedAsync().Wait(); }

        private async Task OnImportsSatisfiedAsync()
        {
            Trace.TraceInformation("Creating Service Bus backplane.");

            this._namespaceManager = NamespaceManager.CreateFromConnectionString(this.Settings.ServiceBusCredentials);

            if (!await _namespaceManager.TopicExistsAsync(this.TopicPath))
            {
                await _namespaceManager.CreateTopicAsync(this.TopicPath);
            }

            if (!await _namespaceManager.SubscriptionExistsAsync(topicPath: this.TopicPath, name: this.SubscriptionName))
            {
                await _namespaceManager.CreateSubscriptionAsync(new SubscriptionDescription(
                    topicPath: this.TopicPath,
                    subscriptionName: this.SubscriptionName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
                });
            }

            this._TopicClient = TopicClient.CreateFromConnectionString(
                connectionString: this.Settings.ServiceBusCredentials,
                path: this.TopicPath);

            this._SubscriptionClient = SubscriptionClient.CreateFromConnectionString(
                connectionString: this.Settings.ServiceBusCredentials,
                topicPath: this.TopicPath,
                name: this.SubscriptionName);

            if (messagesToFetchAtOnce <= 1)
            {
                this.ObservableBackPlane = this._SubscriptionClient.CreateObervable();
            }
            else
            {
                this.ObservableBackPlane = this._SubscriptionClient.CreateObervableBatch(messageCount: messagesToFetchAtOnce);
            }

            Trace.TraceInformation("Service Bus backplane created.");
        }

        private async Task DetachAsync()
        {
            await _namespaceManager.DeleteSubscriptionAsync(
                topicPath: this.TopicPath,
                name: this.SubscriptionName);
        }

        public async Task BroadcastLobbyMessageAsync(BrokeredMessage message)
        {
            await this._TopicClient.SendAsync(message);
        }

        public async Task BroadcastLobbyMessagebatchAsync(IEnumerable<BrokeredMessage> messages)
        {
            await this._TopicClient.SendBatchAsync(messages);
        }

        #region IDisposable

        // Flag: Has Dispose already been called? 
        bool m_disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if (disposing)
            {
                DetachAsync().Wait();
            }

            // Free any unmanaged objects here. 
            //
            m_disposed = true;
        }

        #endregion
    }
}