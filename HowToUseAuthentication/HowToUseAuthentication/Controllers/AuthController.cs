using HowToUseAuthentication.Dtos;
using HowToUseAuthentication.Managers;
using HowToUseAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HowToUseAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyAuthenticationManager _authManager;

        public AuthController(MyAuthenticationManager authManager)
        {
            _authManager = authManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserDto dto)
        {
            if(dto == null)
                return BadRequest("dto is null");

            if (string.IsNullOrWhiteSpace(dto.UserName))
                return BadRequest("userName is empty");

            if (dto.UserName.Length < 3)
                return BadRequest("userName is too short (3 characters minimum)");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("password is empty");

            if (dto.Password.Length < 10)
                return BadRequest("password is too short (10 characters minimum)");

            var model = new UserModel
            {
                UserName = dto.UserName,
                Password = dto.Password,
            };

            var isOk = _authManager.Register(model);

            return isOk ? Ok() : StatusCode(StatusCodes.Status500InternalServerError, "Cannot register user");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(UserDto dto)
        {
            if (dto == null)
                return BadRequest("dto is null");

            var model = new UserModel
            {
                UserName = dto.UserName,
                Password = dto.Password,
            };
            
            var userId = _authManager.FindUserId(model);

            if (!userId.HasValue)
                return NotFound();

            var tokenModel = _authManager.CreateToken(userId.Value);
            var tokenDto = new TokenInfoDto { AccessToken = tokenModel.AccessToken, RefreshToken = tokenModel.RefreshToken };

            return Ok(tokenDto);
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(TokenInfoDto dto)
        {
            if (dto == null)
                return BadRequest("dto is null");

            var model = new TokenInfoModel
            {
                AccessToken = dto.AccessToken,
                RefreshToken = dto.RefreshToken,
            };

            var token = _authManager.GetRefreshedTokenInfo(model);

            if (token == null)
                return BadRequest();

            return Ok(token);
        }
    }
}
