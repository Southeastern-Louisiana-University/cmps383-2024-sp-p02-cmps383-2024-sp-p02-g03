using System.ComponentModel.DataAnnotations;

namespace Selu383.SP24.Api.Features.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }

    public class CreateUserDto
    {
       [Required] public string UserName { get; set; } = string.Empty;
       [Required] public string[] Roles { get; set; } = Array.Empty<string>();
       [Required] public string Password { get; set; } = string.Empty;
    }
}
