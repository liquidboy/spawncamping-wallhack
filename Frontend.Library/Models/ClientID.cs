using System;
namespace Backend.GameLogic.Models
{
    public class ClientID
    {
        public Guid ID { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", typeof(ClientID).Name, this.ID);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ClientID;
            if (other == null) return false;
            return System.Guid.Equals(this.ID, other.ID);
        }

        public override int GetHashCode()
        {
            return typeof(ClientID).GetHashCode() ^ this.ID.GetHashCode();
        }
    }
}
