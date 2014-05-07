namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.Security.Claims;
    using System.ServiceModel.Security.Tokens;

    [Export(typeof(GameAuthenticationHandler))]
    public class GameAuthenticationHandler 
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

    }
}
