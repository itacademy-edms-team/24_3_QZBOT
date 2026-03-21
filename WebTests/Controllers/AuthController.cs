using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using WebTests.Data;
using WebTests.DTOs;
using WebTests.Models;

namespace WebTests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context; 

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                return BadRequest(new IdentityError
                {
                    Code = "DuplicateUserName"
                });
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return BadRequest(new IdentityError
                {
                    Code = "DuplicateEmail"
                });
            }

            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "User created successfully" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);


            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"]))
            });

            return Ok(new { username = user.UserName });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Delete("access_token", cookieOptions);
            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                username = User.Identity!.Name
            });
        }

        [HttpGet("{username}/exist")]
        public async Task<IActionResult> IsUserExists(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
                return Ok(true);
            else return Ok(false);
        }

        [HttpGet("get/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
       {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(new
            {
                id = user.Id,
                status = user.Status,
                username = user.UserName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                avatarUrl = user.AvatarUrl,
                birthDate = user.BirthDate
            });
        }

        [Authorize]
        [HttpPost("edit/{username}")]
        public async Task<IActionResult> EditUserProfile(string username, [FromBody] UserDto dto)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.AvatarUrl = dto.AvatarUrl;
            user.PhoneNumber = dto.PhoneNumber;
            user.Status = dto.Status;
            user.BirthDate = dto.BirthDate;

            await _userManager.UpdateAsync(user);

            return Ok(true);
        }

        public string GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}