namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(AzureGameServerSettings))]
    public class AzureGameServerSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        public string GameServerInstanceId { get { return this.SettingsProvider.GetInstanceId(); } }

        public string ServiceBusCredentials { get { return this.SettingsProvider.GetSetting("ServiceBusCredentials"); } }

        public string GameServerStorageConnectionString { get { return this.SettingsProvider.GetSetting("StorageConnectionString"); } }

        public IPEndPoint IPEndPoint { get { return this.SettingsProvider.GetIPEndpoint("gameServerInstanceEndpoint"); } }
    }
}
