using System.Net;
namespace Backend.GameLogic
{
    public interface ISettingsProvider
    {
        string GetSetting(string key);

        IPEndPoint GetIPEndpoint(string key);

        int GetPublicPort(string key);

        string GetInstanceId();
    }
}
