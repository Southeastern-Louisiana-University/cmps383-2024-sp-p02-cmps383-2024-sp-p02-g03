using Microsoft.AspNetCore.Identity;

namespace Selu383.SP24.Api.Features.Users
{
    public class Role : IdentityRole<int>
    {
        public virtual ICollection<UserRole> Users { get; set; } = new List<UserRole>();
        public virtual ICollection<UserRole> Roles { get; set;}
    }
}
