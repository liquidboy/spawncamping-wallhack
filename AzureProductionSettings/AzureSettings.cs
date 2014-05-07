namespace AzureProductionSettings
{
    using System.ComponentModel.Composition;
    using System.Net;
    using Microsoft.WindowsAzure.ServiceRuntime;

    using Backend.GameLogic;
    using Backend.GameLogic.Configuration;

    [Export(typeof(ISettingsProvider))]
    public class AzureSettings : ISettingsProvider
    {
        string ISettingsProvider.GetInstanceId()
        { 
            return string.Format("{0}-{1}", RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance.Id)
                    .Replace("(", "_").Replace(")", "_")
                    .Replace("deployment", "")
                    .Replace("Cloud.LobbyService.", "")
                    .Replace("Worker", "")
                    ;
        }

        string ISettingsProvider.GetSetting(string key) { return RoleEnvironment.GetConfigurationSettingValue(key); }

        IPEndPoint ISettingsProvider.GetIPEndpoint(string key) { return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[key].IPEndpoint; }

        int ISettingsProvider.GetPublicPort(string key) { return RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[key].PublicIPEndpoint.Port; }
    }
}