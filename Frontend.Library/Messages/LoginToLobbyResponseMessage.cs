namespace Backend.GameLogic.Messages
{
    using System.Net;
    using System.Linq;
    using System.Collections.Generic;
    using System;
    using Frontend.Library.Models;

    public class LoginToLobbyResponseMessage : GameServerMessageBase
    {
        public IPEndPoint GameServer { get; private set; }

        public GameServerID GameServerID { get; private set; }

        public int InnergameServerPort { get; private set; }

        public GameServerUserToken Token { get; private set; }

        public LoginToLobbyResponseMessage() { }

        public LoginToLobbyResponseMessage(IPEndPoint gameServer, int innergameServerPort, GameServerUserToken token, GameServerID gameServerID) 
        {
            this.GameServer = gameServer;
            this.InnergameServerPort = innergameServerPort;
            this.Token = token;
            this.GameServerID = gameServerID;
        }

        public override void PostRead()
        {
            base.PostRead();

            this.GameServer = new IPEndPoint(
                    address: IPAddress.Parse(this.Args[0]),
                    port: int.Parse(this.Args[1]));
            this.InnergameServerPort = int.Parse(this.Args[2]);
            this.Token = new GameServerUserToken
            {
                Credential = this.Args[3]
            };
            this.GameServerID = new GameServerID { ID = Guid.Parse(this.Args[4]) };
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { 
                this.GameServer.Address.ToString(),
                this.GameServer.Port.ToString(),
                this.InnergameServerPort.ToString(),
                this.Token.Credential,
                this.GameServerID.ID.ToString()
            };
        }
    }
}
