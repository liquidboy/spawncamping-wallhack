namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.ServiceModel.Security.Tokens;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Text;

    [Export(typeof(GameAuthenticationHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GameAuthenticationHandler : IPartImportsSatisfiedNotification
    {
        // requires NuGet package System.IdentityModel.Tokens.Jwt

        private static void First()
        {
            var symmetricKey = Convert.FromBase64String("yPsKdKDn4p11L6TEyNhAZCm7P9uum0zLhpE18Y212dc=");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "player123"),
                            new Claim(ClaimTypes.Role, "Player"), 
                        }),
                TokenIssuerName = "lobbyservice",
                AppliesToAddress = "game://server123/game1",
                Lifetime = new Lifetime(
                    created: DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    expires: DateTime.UtcNow.AddMinutes(60)),
                SigningCredentials = new SigningCredentials(
                    new InMemorySymmetricSecurityKey(symmetricKey),
                    "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                    "http://www.w3.org/2001/04/xmlenc#sha256"),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenString = tokenHandler.WriteToken(token);
            Console.WriteLine(tokenString);


            // symmetricKey = Convert.FromBase64String("yPsKdKDn4p11L6TEyNhAZCm7P9uum0zLhpE19Y212dc=");

            var validationParameters = new TokenValidationParameters()
            {
                ValidAudience = "http://www.example.com",
                IssuerSigningToken = new BinarySecretSecurityToken(symmetricKey),
                ValidIssuer = "self"
            };
            tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(tokenString, validationParameters);
        }

        [Import(typeof(BackplaneSettings))]
        public BackplaneSettings BackplaneSettings { get; set; }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            _synchroniedKey = this.EstablishAndRetrieveSyncronizedKey();
        }

        private byte[] _synchroniedKey;

        private CloudBlockBlob GetKeyBlobReference()
        {
            var storageAccount = CloudStorageAccount.Parse(this.BackplaneSettings.StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var gamebackplaneContainer = blobClient.GetContainerReference("gamebackplane");
            gamebackplaneContainer.CreateIfNotExists();
            return  gamebackplaneContainer.GetBlockBlobReference("jwt.key");
        }

        private byte[] GetBlobContents(CloudBlockBlob keyBlob)
        {
            var base64 = keyBlob.DownloadText(encoding: Encoding.UTF8);
            return Convert.FromBase64String(base64);
        }

        private byte[] EstablishAndRetrieveSyncronizedKey()
        {
            var keyBlob = GetKeyBlobReference();
            if (keyBlob.Exists())
            {
                return GetBlobContents(keyBlob);
            } 
            else 
            {
                var newKey = CreateNewKey();
                try
                {
                    var base64 = Convert.ToBase64String(newKey);
                    keyBlob.UploadText(base64, encoding: Encoding.UTF8,  
                        accessCondition: AccessCondition.GenerateIfNoneMatchCondition("*"));
                }
                catch (StorageException) 
                {
                    return GetBlobContents(keyBlob);
                }

                return newKey;
            }
        }

        private byte[] CreateNewKey()
        {
            using (var rng = RNGCryptoServiceProvider.Create())
            {
                var key = new byte[1000];
                rng.GetBytes(key);
                return key;
            }
        }

        private string ComposeAudienceUrl(string gameserverId)
        {
            return string.Format("game://{0}", gameserverId);
        }

        private readonly string _validIssuer = "lobbyservice";

        public string CreatePlayerToken(ClientID clientID, string gameserverId)
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

        public ClientID ValidateClientID(string tokenString, string gameserverId)
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
    }
}
