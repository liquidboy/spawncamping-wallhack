namespace Backend.GameLogic.Security
{
    using System;
    using System.ComponentModel.Composition;
    using System.IdentityModel.Protocols.WSTrust; // requires NuGet package "System.IdentityModel.Tokens.Jwt"
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using Messages;
    using Models;

    /// <summary>
    /// Authenticates JSON Web Tokens in the scope of the game. The LobbyServer issues these tokens, and the GameServer instances consume them. 
    /// </summary>
    public class PlayerAuthenticator
    {
        public PlayerAuthenticator(byte[] secretKey)
        {
            this._synchroniedKey = secretKey;
        }

        public GameServerUserToken CreatePlayerToken(ClientID clientID, string gameserverId)
        {
            var name = clientID.ID.ToString();

            return new GameServerUserToken { Credential = CreatePlayerTokenImpl(name, gameserverId) };
        }

        public ClientID ValidateClientID(GameServerUserToken token, string gameserverId)
        {
            var name = ValidateAndGetNameImpl(token.Credential, gameserverId);

            return new ClientID { ID = int.Parse(name) };
        }

        #region JWT Stuff

        private byte[] _synchroniedKey;

        private string ComposeAudienceUrl(string gameserverId)
        {
            return string.Format("game://{0}", gameserverId);
        }

        private readonly string _validIssuer = "lobbyservice";

        private string CreatePlayerTokenImpl(string name, string gameserverId)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, name),
                            new Claim(ClaimTypes.Role, "Player"), 
                        }),
                TokenIssuerName = _validIssuer,
                AppliesToAddress = ComposeAudienceUrl(gameserverId),
                Lifetime = new Lifetime(
                    created: DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    expires: DateTime.UtcNow.AddMinutes(60)),
                SigningCredentials = new SigningCredentials(
                    new InMemorySymmetricSecurityKey(this._synchroniedKey),
                    "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                    "http://www.w3.org/2001/04/xmlenc#sha256"),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string ValidateAndGetNameImpl(string tokenString, string gameserverId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters()
            {
                ValidAudience = ComposeAudienceUrl(gameserverId),
                IssuerSigningToken = new BinarySecretSecurityToken(this._synchroniedKey),
                ValidIssuer = _validIssuer
            };

            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(tokenString, validationParameters);

                return principal.Claims.First(_ => _.Type == ClaimTypes.NameIdentifier).Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
