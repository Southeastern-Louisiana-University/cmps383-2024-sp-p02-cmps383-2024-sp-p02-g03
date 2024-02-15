using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP24.Api.Features.Users;
using System.Transactions;
using System.Linq.Expressions;
using Selu383.SP24.Api.Features;


namespace Selu383.SP24.Api.Controllers
{
    [ApiController]
    [Route("api/users")]

    public class UsersController : ControllerBase
    {
        
        private readonly UserManager<User> userManager;

        public UsersController(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = Rnames.Admin)]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var NewUser = new User
            {
                UserName = dto.UserName
            };

            var createResult = await userManager.CreateAsync(NewUser, dto.Password); 

            if (!createResult.Succeeded)
            {
                return BadRequest();
            }
            if (dto.Roles == null || dto.Roles.Length == 0)
            {
                return BadRequest();
            }
            try
            {
                var roleResult = await userManager.AddToRolesAsync(NewUser, dto.Roles);
                if (!roleResult.Succeeded)
                {
                    return BadRequest();
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Role") && ex.Message.EndsWith("does not exist."))
            {
                return BadRequest();
            }

            transaction.Complete();

            return Ok(new UserDto
            {
                Id = NewUser.Id,
                Roles = dto.Roles,
                UserName = NewUser.UserName,
            });
        }
    }
}
