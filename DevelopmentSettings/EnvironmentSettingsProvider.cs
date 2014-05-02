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

        private readonly string _LobbyServiceInstanceId = string.Format("dev-{0}", Guid.NewGuid());

        string ILobbyServiceSettings.LobbyServiceInstanceId { get { return _LobbyServiceInstanceId; } }

        IPEndPoint ILobbyServiceSettings.IPEndPoint { get { return new IPEndPoint(IPAddress.Loopback, 3003); } }

        string ILobbyServiceSettings.LobbyStorageConnectionString { get { return S("Lobby.StorageConnectionString"); } }
        
        private string S(string key) { return Environment.GetEnvironmentVariable(key); }
    }
}