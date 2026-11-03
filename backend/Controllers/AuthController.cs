using backend.Contracts;
using backend.Errors;
using backend.Filters;
using backend.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
[TypeFilter(typeof(AuthExceptionFilter))]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet("csrf")]
    [ResponseCache(NoStore = true)]
    public IActionResult Csrf()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    [HttpGet("session")]
    public async Task<IActionResult> Session()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new SessionResponse { Authenticated = false });

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Ok(new SessionResponse { Authenticated = false });

        return Ok(new SessionResponse
        {
            Authenticated = true,
            User = new SessionUserResponse { Id = user.Id, Email = user.Email ?? string.Empty }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim();
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new IdentityValidationException(result.Errors);

        return StatusCode(StatusCodes.Status201Created, new { user.Id, user.Email });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            throw new InvalidCredentialsException();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            throw new InvalidCredentialsException();

        await signInManager.SignInAsync(user, isPersistent: false);
        return Ok(new { user.Id, user.Email });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.GetUserAsync(User);
        return user is null ? Unauthorized() : Ok(new { user.Id, user.Email });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }
}
