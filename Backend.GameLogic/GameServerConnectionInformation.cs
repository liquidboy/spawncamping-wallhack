namespace Backend.GameLogic
{
    using System.Net;

    public class GameServerConnectionInformation
    {
        public GameServerConnectionInformation() { }

        public IPEndPoint GameServer { get; set; }

        public GameServerUserToken Token { get; set; }
    }
}
