namespace AzureProductionSettings
{
    using Backend.GameLogic;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(ISettingsProvider))]
    public class AzureSettings : ISettingsProvider
    {
        string ISettingsProvider.GetInstanceId()
        { 
                var r = RoleEnvironment.DeploymentId + "-" + RoleEnvironment.CurrentRoleInstance.Id;

                r = r.Replace("deployment", "").Replace("(", "_").Replace(")", "_").Replace("Cloud.LobbyService.", ""); 
                // deployment22(3)-deployment22(3).Cloud.LobbyService.Cloud.LobbyService.WorkerRole_IN_0

                return r;
        }

        string ISettingsProvider.GetSetting(string key) { return RoleEnvironment.GetConfigurationSettingValue(key); }

        IPEndPoint ISettingsProvider.GetIPEndpoint(string key) { return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[key].IPEndpoint; }

        int ISettingsProvider.GetPublicPort(string key) { return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[key].PublicIPEndpoint.Port; }
    }
}