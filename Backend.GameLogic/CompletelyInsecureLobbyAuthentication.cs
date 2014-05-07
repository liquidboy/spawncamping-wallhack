namespace Backend.GameLogic
{
    public static class CompletelyInsecureLobbyAuthentication
    {
        private static readonly string _staticPassword = "password from CompletelyInsecureLobbyAuthentication 123";

        public static bool AuthenticatePlayer(ClientID clientID, string password)
        {
            return password == _staticPassword; // @TODO 
        }

        public static string CreatePassword(ClientID clientID) { return _staticPassword; }
    }
}
