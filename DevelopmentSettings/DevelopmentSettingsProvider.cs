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
        private readonly string _LobbyServiceInstanceId = string.Format("dev-{0}", Guid.NewGuid());
        string ISettingsProvider.GetInstanceId() { return _LobbyServiceInstanceId; }

        string ISettingsProvider.GetSetting(string key) { return Environment.GetEnvironmentVariable(key); }

        IPEndPoint ISettingsProvider.GetIPEndpoint(string key) { return new IPEndPoint(IPAddress.Loopback, 3003); }

        int ISettingsProvider.GetPublicPort(string key) { return ((ISettingsProvider)this).GetIPEndpoint(key).Port; }
    }
}