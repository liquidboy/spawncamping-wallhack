namespace Backend.GameLogic
{
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Management.Compute;
    using Microsoft.WindowsAzure.Management.Compute.Models;

    using Configuration;
    using System.Threading;
    using System.Diagnostics;

    [Export(typeof(GameServerScaling))]
    public class GameServerScaling : IPartImportsSatisfiedNotification
    {
        [Import(typeof(LobbyServiceSettings))]
        public LobbyServiceSettings LobbyServiceSettings { get; set; }

        Scaling.ScalingAgent agent;
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            var subscriptionID = this.LobbyServiceSettings.SubscriptionID;
            var subscriptionManagementCertificateThumbprint = this.LobbyServiceSettings.SubscriptionManagementCertificateThumbprint;
            var cloudServiceName = this.LobbyServiceSettings.GameServerCloudServiceName;

            this.agent = new Scaling.ScalingAgent(subscriptionID, subscriptionManagementCertificateThumbprint, cloudServiceName);
        }

        public async Task ScaleAsync()
        {
            await this.agent.ScaleAsync();
        }
    }
}
