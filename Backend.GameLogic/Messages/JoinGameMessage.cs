using System.Collections.Generic;
namespace Backend.GameLogic.Messages
{
    public class JoinGameMessage : GameServerMessageBase
    {
        public int ClientId { get; set; }

        public JoinGameMessage() { }

        public override void PostRead()
        {
            base.PostRead();

            this.ClientId = int.Parse(this.Args[0]);
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { this.ClientId.ToString() };
        }
    }
}
