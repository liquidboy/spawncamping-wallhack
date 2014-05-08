namespace Backend.GameLogic.Configuration
{
    public static class Names
    {
        public static class LobbyRole
        {
            public static readonly string Name = "Cloud.LobbyService.WorkerRole";
            public static class Endpoints
            {
                public static readonly string LobbyService = "LobbyService";
            }
        }
        public static class GameRole
        {
            public static readonly string Name = "Cloud.GameServerHost.WorkerRole";
            public static class Endpoints
            {
                public static readonly string gameServerInstanceEndpoint = "gameServerInstanceEndpoint";
            }
        }
    }
}
