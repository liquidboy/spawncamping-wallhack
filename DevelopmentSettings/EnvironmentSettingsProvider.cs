namespace DevelopmentSettings
{
    using System;
    using System.ComponentModel.Composition;
    using Backend.GameLogic;

    [Export(typeof(ILobbyServiceSettings))]
    public class EnvironmentSettingsProvider : ILobbyServiceSettings
    {
        public string ServiceBusCredentials { get { return Environment.GetEnvironmentVariable("ServiceBusCredentials"); } }

        public string LobbyServiceInstanceId { get { return "dev-" + Guid.NewGuid(); } }
    }
}
