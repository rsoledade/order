using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Order.Infrastructure.Data.Context
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            // Get the directory of the executing assembly
            var assemblyDir = System.IO.Path.GetDirectoryName(typeof(OrderDbContextFactory).Assembly.Location);

            // Navigate up to the solution directory to find the appsettings.json
            var apiProjectDir = System.IO.Path.Combine(assemblyDir, "..", "..", "Order.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName));

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
