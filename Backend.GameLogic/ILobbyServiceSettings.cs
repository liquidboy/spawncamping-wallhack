using System.Net;
namespace Backend.GameLogic
{
    public interface ILobbyServiceSettings
    {
        string ServiceBusCredentials { get; }

        string LobbyStorageConnectionString { get; }

        string LobbyServiceInstanceId { get; }

        IPEndPoint IPEndPoint { get; }
    }
}
