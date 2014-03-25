namespace Cloud.LobbyService.WorkerRole
{
    using System.Net;
    using System.ComponentModel.Composition;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Backend.GameLogic;

    [Export(typeof(ILobbyServiceSettings))]
    public class RoleEnvironmentSettingsProvider : ILobbyServiceSettings
    {
        string ILobbyServiceSettings.LobbyServiceInstanceId 
        { 
            get 
            { 
                var r = RoleEnvironment.DeploymentId + "-" + RoleEnvironment.CurrentRoleInstance.Id;

                r = r.Replace("deployment", "").Replace("(", "_").Replace(")", "_").Replace("Cloud.LobbyService.", ""); 
                // deployment22(3)-deployment22(3).Cloud.LobbyService.Cloud.LobbyService.WorkerRole_IN_0

                return r;
            } 
        }

        IPEndPoint ILobbyServiceSettings.IPEndPoint { get { return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["LobbyService"].IPEndpoint; } }

        string ILobbyServiceSettings.ServiceBusCredentials { get { return S("Lobby.ServiceBusCredentials"); } }

        string ILobbyServiceSettings.LobbyStorageConnectionString { get { return S("Lobby.StorageConnectionString"); } }
        private string S(string key) { return RoleEnvironment.GetConfigurationSettingValue(key); }
    }
}