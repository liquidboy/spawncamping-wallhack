namespace Backend.GameLogic.Messages
{
    using System.Net;
    using System.Linq;
    using System.Collections.Generic;

    public class GameServerConnectionMessage : GameServerMessage
    {
        public IPEndPoint GameServer { get; set; }

        public GameServerUserToken Token { get; set; }

        public GameServerConnectionMessage() { }

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
