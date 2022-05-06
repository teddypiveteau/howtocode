using HowToUseAuthentication.Entities;
using HowToUseAuthentication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HowToUseAuthentication.Managers
{
    public class MyAuthenticationManager
    {
        internal const string AdminRole = "Admin";
        internal const int AccessTokenDurationInMinutes = 1;
        internal const int RefreshTokenDurationInMinutes = 5;

        private List<UserEntity> _users = new List<UserEntity>();
        private List<TokenInfoEntity> _userTokens = new List<TokenInfoEntity>();
        private readonly IConfiguration _configuration;

        public MyAuthenticationManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        internal bool Register(UserModel model)
        {
            if (_users.Any(o => o.UserName.ToLower() == model.UserName.ToLower()))
                return false;

            using (var hmac = new HMACSHA512())
            {
                _users.Add(new UserEntity
                {
                    Id = _users.Select(o => o.Id).OrderByDescending(o => o).FirstOrDefault() + 1,
                    UserName = model.UserName,
                    PasswordSalt = hmac.Key,
                    PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password)),
                });
            }

            return true;
        }

        internal int? FindUserId(UserModel model)
        {
            var entity = _users.FirstOrDefault(o => o.UserName.ToLower() == model.UserName.ToLower());

            if (entity == null)
                return null;

            using (var hmac = new HMACSHA512(entity.PasswordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password));    

                return computedHash.SequenceEqual(entity.PasswordHash) ? entity.Id : null;
            }
        }

        internal TokenInfoModel CreateToken(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, AdminRole),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, _users.FirstOrDefault(o => o.Id == userId).UserName),
            };
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var notBefore = DateTime.Now;
            var expires = DateTime.Now.AddMinutes(AccessTokenDurationInMinutes);
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:TokenPrivateKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims, signingCredentials: credentials, issuer : issuer, audience: audience, notBefore: notBefore, expires: expires);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            _userTokens.Add(new TokenInfoEntity { UserId = userId, AccessToken = accessToken, RefreshToken = refreshToken, RefreshTokenExpirationDate = DateTime.Now.AddMinutes(RefreshTokenDurationInMinutes) }) ;

            return new TokenInfoModel { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        internal TokenInfoModel GetRefreshedTokenInfo(TokenInfoModel oldTokenInfo)
        {
            var tokenPrivateKey = _configuration["Jwt:TokenPrivateKey"];
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenPrivateKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = null;
            SecurityToken securityToken = null;

            try { principal = tokenHandler.ValidateToken(oldTokenInfo.AccessToken, tokenValidationParameters, out securityToken); } catch { }

            if (principal == null || securityToken == null)
                return null;

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase))
                return null;

            if (!int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return null;

            var tokenEntity = _userTokens.FirstOrDefault(t => t.UserId == userId);

            if (tokenEntity == null)
                return null;

            if (tokenEntity.RefreshToken != oldTokenInfo.RefreshToken)
                return null;

            if (DateTime.Now > tokenEntity.RefreshTokenExpirationDate)
                return null;

            _userTokens.Remove(tokenEntity);

            return CreateToken(userId);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
