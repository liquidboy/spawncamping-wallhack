namespace AzureProductionSettings
{
    using System.ComponentModel.Composition;
    using System.Net;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Backend.GameLogic.Configuration;

    [Export(typeof(ISettingsProvider))]
    public class AzureSettings : ISettingsProvider
    {
        internal static class N
        {
            internal static class LobbyRole
            {
                internal static readonly string Name = "Cloud.LobbyService.WorkerRole";
                internal static class Endpoints
                {
                    internal static readonly string LobbyService = "LobbyService";
                }
            }
            internal static class GameRole
            {
                internal static readonly string Name = "Cloud.GameServerHost.WorkerRole";
                internal static class Endpoints
                {
                    internal static readonly string gameServerInstanceEndpoint = "gameServerInstanceEndpoint";
                }
            }
        }

        string ISettingsProvider.GetSetting(string key)
        {
            return RoleEnvironment.GetConfigurationSettingValue(key);
        }

        IPEndPoint ISettingsProvider.LobbyServerInternalEndpoint
        {
            get { return RoleEnvironment.Roles[N.LobbyRole.Name].Instances[0].InstanceEndpoints[N.LobbyRole.Endpoints.LobbyService].IPEndpoint; }
        }

        IPEndPoint ISettingsProvider.GameServerInternalProxyEndpoint
        {
            get { return RoleEnvironment.Roles[N.GameRole.Name].Instances[0].InstanceEndpoints[N.GameRole.Endpoints.gameServerInstanceEndpoint].IPEndpoint; }
        }

        int ISettingsProvider.GameServerPublicProxyPort
        {
            get { return RoleEnvironment.Roles[N.GameRole.Name].Instances[0].InstanceEndpoints[N.GameRole.Endpoints.gameServerInstanceEndpoint].PublicIPEndpoint.Port; }
        }

        IPAddress ISettingsProvider.GameServerPublicAddress
        {
            get { return RoleEnvironment.Roles[N.GameRole.Name].Instances[0].InstanceEndpoints[N.GameRole.Endpoints.gameServerInstanceEndpoint].PublicIPEndpoint.Address; }
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