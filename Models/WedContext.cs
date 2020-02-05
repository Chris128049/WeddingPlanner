using Microsoft.EntityFrameworkCore;


namespace WeddingPlanner.Models
{
    public class MyContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public MyContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Wedding> Wedding { get; set; }
        public DbSet<RSVP> RSVP{ get; set; }
    }
}
