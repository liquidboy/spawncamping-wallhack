namespace Backend.GameLogic.Messages
{
    public class GameServerMessage<T> where T: GameServerMessageBase
    {
        public ErrorMessage ErrorMessage { get; set; }
        public T Message { get; set; }
        public bool IsError { get { return this.ErrorMessage != null; } }

        public GameServerMessage(GameServerMessageBase message)
        {
            var err = message as ErrorMessage;
            if (err != null)
            {
                this.ErrorMessage = err;
                this.Message = null;
            }
            else
            {
                this.ErrorMessage = null;
                this.Message = (T) message;
            }
        }
    }
}
