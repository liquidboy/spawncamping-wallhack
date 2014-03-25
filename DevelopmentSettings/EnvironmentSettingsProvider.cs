namespace DevelopmentSettings
{
    using System;
    using System.ComponentModel.Composition;
    using Backend.GameLogic;
    using System.Net;

    [Export(typeof(ILobbyServiceSettings))]
    public class EnvironmentSettingsProvider : ILobbyServiceSettings
    {
        string ILobbyServiceSettings.ServiceBusCredentials { get { return S("ServiceBusCredentials"); } }

        string ILobbyServiceSettings.LobbyServiceInstanceId { get { return "dev-" + Guid.NewGuid(); } }

        IPEndPoint ILobbyServiceSettings.IPEndPoint { get { return new IPEndPoint(IPAddress.Loopback, 3000); } }

        string ILobbyServiceSettings.LobbyStorageConnectionString { get { return S("Lobby.StorageConnectionString"); } }
        
        private string S(string key) { return Environment.GetEnvironmentVariable(key); }
    }
}