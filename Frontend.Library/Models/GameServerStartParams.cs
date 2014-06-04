namespace Frontend.Library.Models
{
    using System;

    public class GameServerStartParams
    {
        public GameServerID GameServerID { get; set; }
    }

    public class GameServerID
    {
        public Guid ID { get; set; }
    }
}
