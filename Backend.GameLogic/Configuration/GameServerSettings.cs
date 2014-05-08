namespace Backend.GameLogic.Configuration
{
    using System.ComponentModel.Composition;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;

    [Export(typeof(GameServerSettings))]
    public class GameServerSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        [Import(typeof(SharedSettings))]
        public SharedSettings SharedSettings { get; set; }

        public IPEndPoint ProxyIPEndPoint { get { return this.SettingsProvider.GameServerInternalProxyEndpoint; } }

        public int PublicGameServerPort { get { return this.SettingsProvider.GameServerPublicProxyPort; } }
    }
}
