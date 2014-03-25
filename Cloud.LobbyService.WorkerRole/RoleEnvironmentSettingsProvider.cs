namespace Cloud.LobbyService.WorkerRole
{
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.ComponentModel.Composition;
    using Backend.GameLogic;
    using System.Net;

    [Export(typeof(ILobbyServiceSettings))]
    public class RoleEnvironmentSettingsProvider : ILobbyServiceSettings
    {
        string ILobbyServiceSettings.LobbyServiceInstanceId { get { return RoleEnvironment.DeploymentId + "-" + RoleEnvironment.CurrentRoleInstance.Id; } }

        IPEndPoint ILobbyServiceSettings.IPEndPoint { get { return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["LobbyService"].IPEndpoint; } }

        string ILobbyServiceSettings.ServiceBusCredentials { get { return S("Lobby.ServiceBusCredentials"); } }

        string ILobbyServiceSettings.LobbyStorageConnectionString { get { return S("Lobby.StorageConnectionString"); } }
        private string S(string key) { return RoleEnvironment.GetConfigurationSettingValue(key); }
    }
}