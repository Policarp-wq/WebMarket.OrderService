using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebMarket.OrderService.Auth
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtOptions _options;

        public JwtProvider(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public const int EXPIRATION_HOURS = 24;
        private const string LOGIN_CLAIM = "Login";
        private const string ROLE_CLAIM = "Role";

        public string GenerateToken(ApplicationUser user)
        {
            var claims = new Claim[]
            {
                new Claim(LOGIN_CLAIM, user.Login),
                new Claim(ROLE_CLAIM, user.Role.ToCustomString())
            };
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _options.KeyIssuer,
                _options.Audience,
                claims,
                null,
                DateTime.UtcNow.AddHours(EXPIRATION_HOURS),
                signingCredentials
                );
            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token); 
            return tokenValue;
        }
    }
}
