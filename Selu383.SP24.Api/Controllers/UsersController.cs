using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP24.Api.Features.Users;

namespace Selu383.SP24.Api.Controllers
{
    [Route("api/users")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
        {
            var NewUser = new User
            {
                UserName = dto.UserName
            };

            return Ok(new UserDto
            {
                Id = NewUser.Id,
                Roles = dto.Roles,
                UserName = NewUser.UserName,
            });
        }
    }
}
