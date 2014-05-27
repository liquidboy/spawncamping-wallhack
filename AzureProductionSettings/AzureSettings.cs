namespace AzureProductionSettings
{
    using System.ComponentModel.Composition;
    using System.Net;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Backend.GameLogic.Configuration;

    [Export(typeof(ISettingsProvider))]
    public class AzureSettings : ISettingsProvider
    {
        string ISettingsProvider.GetSetting(string key)
        {
            return RoleEnvironment.GetConfigurationSettingValue(key);
        }

        IPEndPoint ISettingsProvider.LobbyServerInternalEndpoint
        {
            get { return RoleEnvironment.Roles[Names.LobbyRole.Name].Instances[0].InstanceEndpoints[Names.LobbyRole.Endpoints.LobbyService].IPEndpoint; }
        }

        IPEndPoint ISettingsProvider.GameServerInternalProxyEndpoint
        {
            get { return RoleEnvironment.Roles[Names.GameRole.Name].Instances[0].InstanceEndpoints[Names.GameRole.Endpoints.gameServerInstanceEndpoint].IPEndpoint; }
        }

        int ISettingsProvider.GameServerPublicProxyPort
        {
            get { return RoleEnvironment.Roles[Names.GameRole.Name].Instances[0].InstanceEndpoints[Names.GameRole.Endpoints.gameServerInstanceEndpoint].PublicIPEndpoint.Port; }
        }

        IPAddress ISettingsProvider.GameServerPublicAddress
        {
            get { return RoleEnvironment.Roles[Names.GameRole.Name].Instances[0].InstanceEndpoints[Names.GameRole.Endpoints.gameServerInstanceEndpoint].PublicIPEndpoint.Address; }
        }

        string ISettingsProvider.InstanceId
        {
            get
            {
                return string.Format("{0}-{1}", RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance.Id)
                    .Replace("(", "_").Replace(")", "_")
                    .Replace("deployment", "")
                    .Replace("Cloud.LobbyService.", "")
                    .Replace("Worker", "")
                    ;
            }
        }
    }
}