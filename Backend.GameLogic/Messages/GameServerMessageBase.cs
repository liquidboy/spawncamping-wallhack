namespace Backend.GameLogic.Messages
{
    using System.Collections.Generic;
    using System.Net;

    public class GameServerMessageBase
    {
        public string Command { get; set; }

        public IList<string> Args { get; set; }

        internal GameServerMessageBase() 
        {
            this.Command = this.GetType().Name;
            this.Args = new List<string>(); 
        }

        public GameServerMessageBase(string command, params string[] args)
        {
            this.Command = command;
            this.Args = new List<string>(args);
        }

        public virtual void PostRead() 
        {
            if (this.Command != this.GetType().Name)
            {
                throw new ProtocolViolationException(string.Format("Command mismatch: Expected {0} but received {1}",
                    this.GetType().Name, this.Command));
            }
        }

        public virtual void PreWrite() 
        {
            this.Command = this.GetType().Name;
        }
    }
}