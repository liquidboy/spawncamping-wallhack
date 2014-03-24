namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.Composition;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    [Export(typeof(LobbyServiceBackplane))]
    public class LobbyServiceBackplane : IPartImportsSatisfiedNotification
    {
        [Import("ServiceBusCredentials")]
        public string ServiceBusCredentials { get; set; }

        [Import("LobbyServiceInstanceId")]
        public string LobbyServiceInstanceId { get; set; }

        public LobbyServiceBackplane() { }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() { }

        private const string LobbyServiceTopic = "LobbyServiceTopic";

        public async Task EnsureSetupAsync()    
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(this.ServiceBusCredentials);

            if (!await namespaceManager.TopicExistsAsync(LobbyServiceTopic))
            {
                await namespaceManager.CreateTopicAsync(LobbyServiceTopic);
            }

            await namespaceManager.CreateSubscriptionAsync(new SubscriptionDescription(
                topicPath: LobbyServiceTopic, 
                subscriptionName: this.LobbyServiceInstanceId)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
            });
        }
    }
}
