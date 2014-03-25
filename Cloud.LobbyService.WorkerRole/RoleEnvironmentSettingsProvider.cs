namespace Cloud.LobbyService.WorkerRole
{
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.ComponentModel.Composition;
    using Backend.GameLogic;

    [Export(typeof(ILobbyServiceSettings))]
    public class RoleEnvironmentSettingsProvider : ILobbyServiceSettings
    {
        private string S(string key) { return RoleEnvironment.GetConfigurationSettingValue(key); }
        
        public string ServiceBusCredentials { get { return S("ServiceBusCredentials"); } }

        public string LobbyServiceInstanceId { get { return RoleEnvironment.DeploymentId + "-" + RoleEnvironment.CurrentRoleInstance.Id; } }
    }
}