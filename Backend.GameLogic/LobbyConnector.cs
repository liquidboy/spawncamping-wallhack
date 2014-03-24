namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.ComponentModel.Composition;
    using Microsoft.ServiceBus;

    [Export(typeof(LobbyConnector))]
    public class LobbyConnector : IPartImportsSatisfiedNotification
    {
        [Import("ServiceBusCredentials")]
        public string ServiceBusCredentials { get; set; }

        public LobbyConnector() { }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() { }

        public async Task EnsureSetupAsync()    
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(this.ServiceBusCredentials);

            if (! await namespaceManager.TopicExistsAsync("TestTopic"))
            {
                await namespaceManager.CreateTopicAsync("TestTopic");
            }
        }
    }
}
