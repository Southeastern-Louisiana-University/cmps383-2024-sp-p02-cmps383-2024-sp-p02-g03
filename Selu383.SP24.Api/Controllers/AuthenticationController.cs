using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Features.Users;

namespace Selu383.SP24.Api.Controllers;


public class AuthenticationController : ControllerBase
{

    private readonly DbSet<User> users;
    private readonly DataContext dataContext;
    public AuthenticationController(DataContext dataContext)
    {
        this.dataContext = dataContext;
        users = dataContext.Set<User>();
    }

    /*[HttpPost]
    [Route("api/authentication/login")]
    public async Task<IActionResult> Login(LoginDto login)
    {


    }*/

    [HttpGet]
    [Route("api/authentication/me")]
    public IQueryable<UserDto> Me()
    {
        return GetUserDtos(users);
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
