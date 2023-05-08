using Microsoft.EntityFrameworkCore;
using Tofu.BallBoi.Abstractions.DataTransferObjects;
using Tofu.BallBoi.Core;

namespace Tofu.BallBoi.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<UserDTO> Users { get; set; }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
