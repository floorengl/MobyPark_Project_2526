using Microsoft.EntityFrameworkCore;

namespace CS_api.Models
{
    public class MobyParkDBContext : DbContext
    {
        public MobyParkDBContext(DbContextOptions<MobyParkDBContext> options) : base(options)
        { 

        }

        public DbSet<Test> Test { get; set; } //voorbeeld
    }
}
