using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Features.Users;
using Microsoft.AspNetCore.Authorization;

namespace Selu383.SP24.Api.Controllers;


public class AuthenticationController : ControllerBase
{

    private readonly DbSet<User> users;
    private readonly DataContext dataContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    public AuthenticationController(DataContext dataContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        this.dataContext = dataContext;
        users = dataContext.Set<User>();
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    [Route("api/authentication/login")]
    public async Task<IActionResult> Login(LoginDto login)
    {

        var result = await _signInManager.PasswordSignInAsync(login.UserName, login.Password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // Authentication successful, create UserDto with the same information
            var userDto = new UserDto { UserName = login.UserName };
            
            return Ok(userDto);
        }
        else
        {
            // Authentication failed, return bad request
            return BadRequest("Invalid username or password");
        }
        
    }

    [HttpGet]
    [Route("api/authentication/me")]
    public IActionResult GetCurrentUser()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;

        if (identity == null) { return Unauthorized(); }

        var usernameClaim = identity.FindFirst(ClaimTypes.Name);
        var username = usernameClaim?.Value;

        return Ok(new {Username = username});
    }

    [HttpPost, Route("/api/authentication/logout")]
    [Authorize]

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        
        return Ok();
    }

    private static IQueryable<UserDto> GetUserDtos(IQueryable<User> users) 
    {
        return users
            .Select(x => new UserDto
            {
                Id = x.Id,
                UserName = x.UserName,
            });
    }


}
