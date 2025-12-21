using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Modules.Auth.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NotesApp.API.Modules.Auth.Utility
{
    public class TokenProvider(IOptions<JwtSettings> options) : ITokenProvider
    {
        private readonly JwtSettings settings = options.Value;

        public string CreateToken(Guid id, string email, string role)
        {
            byte[] keyBytes = Convert.FromBase64String(settings.Key);
            SymmetricSecurityKey securityKey = new(keyBytes);
            List<Claim> claims =
            [
                new("id", id.ToString()),
                new("email", email),
                new(ClaimTypes.Role, role)
            ];
            SigningCredentials cred = new(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new(issuer: settings.Issuer,
                audience: settings.Audience, 
                claims: claims, 
                expires: DateTime.UtcNow.AddHours(settings.ExpiresInHours), 
                signingCredentials: cred);
            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public bool ValidateToken(string token)
        {
            try
            {
                byte[] keyBytes = Convert.FromBase64String(settings.Key);
                SymmetricSecurityKey securityKey = new(keyBytes);

                TokenValidationParameters validationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                _ = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken? validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public ClaimsPrincipal? GetClaimsFromToken(string token)
        {
            try
            {
                byte[] keyBytes = Convert.FromBase64String(settings.Key);
                SymmetricSecurityKey securityKey = new(keyBytes);

                TokenValidationParameters validationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken? validatedToken);
            }
            catch
            {
                return null;
            }
        }
    }
}