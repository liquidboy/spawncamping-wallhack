namespace Backend.GameLogic
{
    using System;
    using System.Net;

    public class ClientState
    {
        public ClientID ClientID { get; set; }

        public string LobbyServerInstance { get; set; }

        public IPAddress IPAddress { get; set; }

        public DateTimeOffset ConnectedSince { get; set; }
    }
}
