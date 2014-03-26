namespace Backend.GameLogic.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ErrorMessage : GameServerMessageBase
    {
        public string Message { get; set; }

        public ErrorMessage(string errorMessage)
        {
            this.Command = this.GetType().Name;
            this.Message = errorMessage;
        }
        public ErrorMessage() { }

        public override void PostRead()
        {
            base.PostRead();

            this.Message = this.Args[0];
        }

        public override void PreWrite()
        {
            base.PreWrite();

            this.Args = new List<string> { this.Message };
        }
    }
}