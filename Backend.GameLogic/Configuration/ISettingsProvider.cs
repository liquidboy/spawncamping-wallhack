namespace Backend.GameLogic.Configuration
{
    using System.Net;

    public interface ISettingsProvider
    {
        string GetSetting(string key);

        IPEndPoint LobbyServerInternalEndpoint { get; }

        IPEndPoint GameServerInternalProxyEndpoint { get; }

        int GameServerPublicProxyPort { get; }

        IPAddress GameServerPublicAddress { get; }

        string InstanceId { get; }
    }
}