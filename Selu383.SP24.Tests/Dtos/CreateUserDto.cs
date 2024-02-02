namespace Selu383.SP24.Tests.Dtos;

internal class CreateUserDto
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string[]? Roles { get; set; }
}
