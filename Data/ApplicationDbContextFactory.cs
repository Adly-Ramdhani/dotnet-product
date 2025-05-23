using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductManagementApp.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Ganti dengan koneksi PostgreSQL kamu
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=db_product;Username=postgres;Password=nabati321");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
