namespace Backend.GameLogic.Messages
{
     using System.Net;
    using System.Linq;
    using System.Collections.Generic;

    public class LoginToGameServerRequest : GameServerMessageBase
    {
        public GameServerUserToken Token { get; private set; }

        public LoginToGameServerRequest() { }

        public LoginToGameServerRequest(GameServerUserToken token) 
        {
            this.Token = token;
        }

        public override void PostRead()
        {
            base.PostRead();

            this.Token = new GameServerUserToken
            {
                Credential = this.Args[0]
            };
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { 
                this.Token.Credential
            };
        }
    }
}
