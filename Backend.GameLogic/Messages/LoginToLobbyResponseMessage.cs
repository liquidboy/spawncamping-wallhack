namespace Backend.GameLogic.Messages
{
    using System.Net;
    using System.Linq;
    using System.Collections.Generic;

    public class LoginToLobbyResponseMessage : GameServerMessageBase
    {
        public IPEndPoint GameServer { get; private set; }

        public GameServerUserToken Token { get; private set; }

        public LoginToLobbyResponseMessage() { }

        public LoginToLobbyResponseMessage(IPEndPoint gameServer, GameServerUserToken token) 
        {
            this.GameServer = gameServer;
            this.Token = token;
        }

        public override void PostRead()
        {
            base.PostRead();

            this.GameServer = new IPEndPoint(
                    address: IPAddress.Parse(this.Args[0]),
                    port: int.Parse(this.Args[1]));
            this.Token = new GameServerUserToken
            {
                Credential = this.Args[2]
            };
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { 
                this.GameServer.Address.ToString(),
                this.GameServer.Port.ToString(),
                this.Token.Credential
            };
        }
    }
}
