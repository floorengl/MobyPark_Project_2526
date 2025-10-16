using System.ComponentModel.DataAnnotations;

public sealed class LicenseplateDto
{
    [Required, MaxLength(10)] public string? LicensePlateName { get; set; }
}