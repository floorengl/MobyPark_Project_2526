namespace MobyPark_api.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}
