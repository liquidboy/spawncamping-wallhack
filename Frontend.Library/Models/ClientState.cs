namespace Backend.GameLogic.Models
{
    using System;
    using System.Net;
    using Models;

    public class ClientState
    {
        public ClientID ClientID { get; set; }

        public string LobbyServerInstance { get; set; }

        public IPAddress IPAddress { get; set; }

        public DateTimeOffset ConnectedSince { get; set; }
    }
}
