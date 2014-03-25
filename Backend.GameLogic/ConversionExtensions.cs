namespace Backend.GameLogic
{
    using System.Net;

    public static class ConversionExtensions
    {

        public static GameServerMessage AsGameServerMessage(this GameServerConnectionInformation gameServerConnectionInformation)
        {
            return new GameServerMessage(
                "LobbyInformation",
                gameServerConnectionInformation.GameServer.Address.ToString(),
                gameServerConnectionInformation.GameServer.Port.ToString(),
                gameServerConnectionInformation.Token.Credential);
        }

        public static GameServerConnectionInformation AsGameServerConnectionInformation(this GameServerMessage command)
        {
            if (command.Command != "LobbyInformation")
            {
                throw new ProtocolViolationException("Could not retrieve LobbyInformation");
            }

            return new GameServerConnectionInformation
            {
                GameServer = new IPEndPoint(
                    address: IPAddress.Parse(command.Args[0]),
                    port: int.Parse(command.Args[1])),
                Token = new GameServerUserToken
                {
                    Credential = command.Args[2]
                }
            };
        }
    }
}
