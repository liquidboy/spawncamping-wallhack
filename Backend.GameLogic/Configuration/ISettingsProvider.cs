namespace Backend.GameLogic.Configuration
{
    using System.Net;

    public interface ISettingsProvider
    {
        string GetSetting(string key);

        IPEndPoint GetIPEndpoint(string key);

        int GetPublicPort(string key);

        string GetInstanceId();
    }
}
