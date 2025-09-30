using System.ComponentModel.DataAnnotations;

public sealed class RegisterRequestDto
{
    [Required, StringLength(50)] public string Username { get; set; } = "";
    [Required, MinLength(8)]     public string Password { get; set; } = "";
}
