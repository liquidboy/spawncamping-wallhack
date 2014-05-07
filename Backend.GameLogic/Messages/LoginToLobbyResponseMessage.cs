﻿namespace Backend.GameLogic.Messages
{
    using System.Net;
    using System.Linq;
    using System.Collections.Generic;

    public class LoginToLobbyResponseMessage : GameServerMessageBase
    {
        public IPEndPoint GameServer { get; private set; }

        public int InnergameServerPort { get; private set; }

        public GameServerUserToken Token { get; private set; }

        public LoginToLobbyResponseMessage() { }

        public LoginToLobbyResponseMessage(IPEndPoint gameServer, int innergameServerPort, GameServerUserToken token) 
        {
            this.GameServer = gameServer;
            this.InnergameServerPort = innergameServerPort;
            this.Token = token;
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
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { 
                this.GameServer.Address.ToString(),
                this.GameServer.Port.ToString(),
                this.InnergameServerPort.ToString(),
                this.Token.Credential
            };
        }
    }
}
