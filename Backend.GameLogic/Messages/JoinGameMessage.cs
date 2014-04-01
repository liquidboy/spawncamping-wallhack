using System.Collections.Generic;
namespace Backend.GameLogic.Messages
{
    public class JoinGameMessage : GameServerMessageBase
    {
        public ClientID ClientID { get; set; }

        public JoinGameMessage() { }

        public override void PostRead()
        {
            base.PostRead();

            this.ClientID = new ClientID { ID = int.Parse(this.Args[0]) };
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { this.ClientID.ID.ToString() };
        }
    }
}
