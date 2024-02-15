using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Features.Users;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Selu383.SP24.Api.Controllers;

[ApiController]
[Route("api/authentication")]

public class AuthenticationController : ControllerBase
{

    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;

    public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var username = GetCurrentUserName(User);

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var resultDto = await GetUserDto(userManager.Users).SingleAsync(x => x.UserName == username);
        return Ok(resultDto);
    }

    [HttpPost]
    [Route("login")]

    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
        {
            return BadRequest("Username Not Valid");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, true);

        if (!result.Succeeded)
        {
            return BadRequest("Password Not Valid");
        }

        await signInManager.SignInAsync(user, false);

        var resultDto = await GetUserDto(userManager.Users).SingleAsync(x => x.UserName == user.UserName);

        return Ok(resultDto);
    }

    [HttpPost]
    [Route("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return Ok();
    }

    private static IQueryable<UserDto> GetUserDto(IQueryable<User> users) 
    {
        return users
            .Select(x => new UserDto
            {
                Id = x.Id,
                UserName = x.UserName!,
                Roles = x.Roles.Select(y => y.Role!.Name).ToArray()!,
            });
    }
    private string? GetCurrentUserName(ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Identity?.Name;
    }
}
