namespace DevelopmentSettings
{
    using System;
    using System.ComponentModel.Composition;
    using System.Net;
    using Backend.GameLogic;
    using Backend.GameLogic.Configuration;

    [Export(typeof(ISettingsProvider))]
    public class DevelopmentSettingsProvider : ISettingsProvider
    {
        string ISettingsProvider.GetSetting(string key) { return Environment.GetEnvironmentVariable(key); }

        IPEndPoint ISettingsProvider.LobbyServerInternalEndpoint
        {
            get { return new IPEndPoint(IPAddress.Loopback, 3000); }
        }

        IPEndPoint ISettingsProvider.GameServerInternalProxyEndpoint
        {
            get { return new IPEndPoint(IPAddress.Loopback, 4000); }
        }

        int ISettingsProvider.GameServerPublicProxyPort
        {
            get { return 4000; }
        }

        IPAddress ISettingsProvider.GameServerPublicAddress
        {
            get { return IPAddress.Loopback; }
        }

        private readonly string _LobbyServiceInstanceId = string.Format("dev-{0}", Guid.NewGuid());
        string ISettingsProvider.InstanceId { get { return _LobbyServiceInstanceId; } }
    }
}