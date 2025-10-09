using System.ComponentModel.DataAnnotations;

namespace MobyPark_api.Data.Models
{
    public class Vehicle
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public required string LicensePlate { get; set; }

        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public DateTime? Year { get; set; }

        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public long UserId { get; set; }
        public User? User { get; set; }


        public DateTime CreatedAt { get; set; }
    }
}
