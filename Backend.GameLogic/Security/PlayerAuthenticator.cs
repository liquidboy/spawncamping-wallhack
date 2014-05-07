namespace Backend.GameLogic.Security
{
    using System;
    using System.ComponentModel.Composition;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using Backend.GameLogic.Messages;

    public class PlayerAuthenticator
    {
        private byte[] _synchroniedKey;

        public PlayerAuthenticator(byte[] secretKey)
        {
            this._synchroniedKey = secretKey;
        }

        private string ComposeAudienceUrl(string gameserverId)
        {
            return string.Format("game://{0}", gameserverId);
        }

        private readonly string _validIssuer = "lobbyservice";

        private string CreatePlayerTokenImpl(ClientID clientID, string gameserverId)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, clientID.ID.ToString()),
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

            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        private ClientID ValidateClientIdImpl(string tokenString, string gameserverId)
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

                var clientIdValue = principal.Claims.First(_ => _.Type == ClaimTypes.NameIdentifier).Value;

                return new ClientID { ID = int.Parse(clientIdValue) };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public GameServerUserToken CreatePlayerToken(ClientID clientID, string gameserverId)
        {
            return new GameServerUserToken { Credential = CreatePlayerTokenImpl(clientID, gameserverId) };
        }

        public ClientID ValidateClientID(GameServerUserToken token, string gameserverId)
        {
            return ValidateClientIdImpl(token.Credential, gameserverId);
        }

    }
}
