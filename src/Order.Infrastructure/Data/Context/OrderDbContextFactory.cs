using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;

namespace Order.Infrastructure.Data.Context
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            // Busca recursiva por appsettings.json subindo na hierarquia de diretórios
            var basePath = Directory.GetCurrentDirectory();
            string? configDir = FindConfigDirectory(basePath, "appsettings.json");

            // Tenta explicitamente src/Order.API a partir da raiz do repositório
            if (configDir == null)
            {
                var repoRoot = FindRepoRoot(basePath);
                if (repoRoot != null)
                {
                    var tryApi = Path.Combine(repoRoot, "src", "Order.API");
                    if (File.Exists(Path.Combine(tryApi, "appsettings.json")))
                        configDir = tryApi;
                }
            }

            if (configDir == null)
            {
                throw new FileNotFoundException(
                    $"Não foi possível localizar o appsettings.json. " +
                    $"Execute o comando a partir da raiz do projeto ou de src/Order.API. " +
                    $"Diretório atual: {basePath}");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(configDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName));

            return new OrderDbContext(optionsBuilder.Options);
        }

        // Busca recursiva por um arquivo subindo na hierarquia de diretórios
        private static string? FindConfigDirectory(string startPath, string fileName)
        {
            var dir = new DirectoryInfo(startPath);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, fileName);
                if (File.Exists(candidate))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        // Busca a raiz do repositório (onde está a pasta src)
        private static string? FindRepoRoot(string startPath)
        {
            var dir = new DirectoryInfo(startPath);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "src")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }
    }
}
