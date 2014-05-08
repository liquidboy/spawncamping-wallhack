namespace Backend.GameLogic.Configuration
{
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(LobbyServiceSettings))]
    public class LobbyServiceSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        [Import(typeof(SharedSettings))]
        public SharedSettings SharedSettings { get; set; }

        public IPEndPoint IPEndPoint { get { return this.SettingsProvider.LobbyServerInternalEndpoint; } }

        public IPEndPoint GameServerEndpoint { get { return new IPEndPoint(
            this.SettingsProvider.GameServerPublicAddress,
            this.SettingsProvider.GameServerPublicProxyPort); } }

        public string SubscriptionID { get { return this.SettingsProvider.GetSetting("SubscriptionID"); } }

        public string SubscriptionManagementCertificateThumbprint { get { return this.SettingsProvider.GetSetting("SubscriptionManagementCertificateThumbprint"); } }

        public string GameServerCloudServiceName { get { return this.SettingsProvider.GetSetting("GameServerCloudServiceName"); } }
    }
}
