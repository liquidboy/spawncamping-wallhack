namespace Backend.GameLogic
{
    using Backend.GameLogic;
    using System;
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(LobbyServiceSettings))]
    public class LobbyServiceSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        public string LobbyServiceInstanceId { get { return this.SettingsProvider.GetInstanceId(); } }

        public IPEndPoint IPEndPoint { get { return this.SettingsProvider.GetIPEndpoint("LobbyService"); } }

        public string LobbyStorageConnectionString { get { return this.SettingsProvider.GetSetting("StorageConnectionString"); } }

        public string ServiceBusCredentials { get { return this.SettingsProvider.GetSetting("ServiceBusCredentials"); } }
    }
}
