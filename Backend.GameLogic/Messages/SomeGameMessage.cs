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

        public override void PostRead()
        {
            base.PostRead();

            this.Stuff = this.Args[0];
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { 
                this.Stuff 
            };
        }
    }
}
