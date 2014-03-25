namespace Backend.GameLogic
{
    public interface ILobbyServiceSettings
    {
        string ServiceBusCredentials { get; }

        string LobbyServiceInstanceId { get; }
    }
}
