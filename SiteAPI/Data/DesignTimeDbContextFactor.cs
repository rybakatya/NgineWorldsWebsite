using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthApi.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conn = cfg.GetConnectionString("DefaultConnection")
            ?? "Server=127.0.0.1;Port=3306;Database=AuthApiDb;User=root;Password=YOUR_PWD;TreatTinyAsBoolean=false;CharSet=utf8mb4;";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(conn, ServerVersion.AutoDetect(conn))
            .Options;

        return new ApplicationDbContext(options);
    }
}
