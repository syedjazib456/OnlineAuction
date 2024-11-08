using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Online_Auction.Models
{
    public class ApplicationDbContext:IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Register> Register { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Ratings> Ratings { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Bids> Bids { get; set; }
    }
}
