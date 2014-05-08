namespace Backend.GameLogic.Models
{
    public class ClientID
    {
        public int ID { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", typeof(ClientID).Name, this.ID);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ClientID;
            if (other == null) return false;
            return this.ID == other.ID;
        }

        public override int GetHashCode()
        {
            return typeof(ClientID).GetHashCode() ^ this.ID.GetHashCode();
        }
    }
}
