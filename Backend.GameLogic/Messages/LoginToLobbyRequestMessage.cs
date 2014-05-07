namespace Backend.GameLogic.Messages
{
    using System.Collections.Generic;

    public class LoginToLobbyRequestMessage : GameServerMessageBase
    {
        public ClientID ClientID { get; private set; }

        public string Password { get; private set; }

        public LoginToLobbyRequestMessage() { }

        public LoginToLobbyRequestMessage(ClientID clientID, string password) 
        {
            this.ClientID = clientID;
            this.Password = password;
        }

        public override void PostRead()
        {
            base.PostRead();

            this.ClientID = new ClientID { ID = int.Parse(this.Args[0]) };
            this.Password = this.Args[1];
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> 
            { 
                this.ClientID.ID.ToString(),
                this.Password
            };
        }
    }
}
