using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ASI.Basecode.Data
{
    public class AsiBasecodeDbContextFactory : IDesignTimeDbContextFactory<AsiBasecodeDBContext>
    {
        public AsiBasecodeDBContext CreateDbContext(string[] args)
        {
            // Find and load the appsettings.json from the WebApp project
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ASI.Basecode.WebApp");
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Get connection string
            var connectionString = config.GetConnectionString("DefaultConnection");

            // Configure DbContext
            var options = new DbContextOptionsBuilder<AsiBasecodeDBContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AsiBasecodeDBContext(options);
        }
    }
}
