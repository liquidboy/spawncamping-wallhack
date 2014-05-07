namespace Backend.GameLogic.Configuration
{
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(GameServerSettings))]
    public class GameServerSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        [Import(typeof(BackplaneSettings))]
        public BackplaneSettings BackplaneSettings { get; set; }

        public IPEndPoint ProxyIPEndPoint { get { return this.SettingsProvider.GetIPEndpoint("gameServerInstanceEndpoint"); } }

        public int PublicGameServerPort { get { return this.SettingsProvider.GetPublicPort("gameServerInstanceEndpoint"); } }
    }
}
