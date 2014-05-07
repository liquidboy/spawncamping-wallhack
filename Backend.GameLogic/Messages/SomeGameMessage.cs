namespace Backend.GameLogic.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SomeGameMessage : GameServerMessageBase
    {
        public SomeGameMessage() { }

        public string Stuff { get; set; }

        public ClientID From { get; set; }

        public override void PostRead()
        {
            base.PostRead();

            this.Stuff = this.Args[0];
            this.From = new ClientID{ ID = int.Parse(this.Args[1]) };
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { 
                this.Stuff, 
                this.From.ID.ToString()
            };
        }
    }
}
