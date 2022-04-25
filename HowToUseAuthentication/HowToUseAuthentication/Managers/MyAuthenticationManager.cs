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

        private List<UserEntity> _users = new List<UserEntity>();
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

        internal string CreateToken(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, AdminRole),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, _users.FirstOrDefault(o => o.Id == userId).UserName),
            };
            var tokenPrivateKey = _configuration.GetSection("AppSettings:TokenPrivateKey").Value;
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenPrivateKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims, signingCredentials: credentials, expires: DateTime.Now.AddMinutes(15));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
