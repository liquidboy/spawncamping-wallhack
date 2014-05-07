namespace Backend.GameLogic.Configuration
{
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(LobbyServiceSettings))]
    public class LobbyServiceSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        [Import(typeof(BackplaneSettings))]
        public BackplaneSettings BackplaneSettings { get; set; }

        public IPEndPoint IPEndPoint { get { return this.SettingsProvider.LobbyServerInternalEndpoint; } }

        public IPEndPoint GameServerEndpoint { get { return new IPEndPoint(
            this.SettingsProvider.GameServerPublicAddress,
            this.SettingsProvider.GameServerPublicProxyPort); } }
    }
}
