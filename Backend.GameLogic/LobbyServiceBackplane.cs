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

    [Export(typeof(LobbyServiceBackplane))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LobbyServiceBackplane : IPartImportsSatisfiedNotification, IDisposable
    {
        [Import(typeof(ILobbyServiceSettings))]
        public ILobbyServiceSettings Settings { get; set; }

        public IObservable<BrokeredMessage> ObservableBackPlane { get; private set; }

        public string TopicPath { get { return "LobbyServiceTopic"; } }
        public string SubscriptionName { get { return this.Settings.LobbyServiceInstanceId; } }
        private NamespaceManager _namespaceManager;
        private SubscriptionClient _SubscriptionClient;
        private TopicClient _TopicClient;

        public LobbyServiceBackplane() { }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() { this.OnImportsSatisfiedAsync().Wait(); }

        private async Task OnImportsSatisfiedAsync()    
        {
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

            this.ObservableBackPlane = this._SubscriptionClient.CreateObervable();
        }

        public async Task DetachAsync()
        {
            await _namespaceManager.DeleteSubscriptionAsync(
                topicPath: this.TopicPath,
                name: this.SubscriptionName);
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