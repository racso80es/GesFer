using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GesFer.Admin.Infrastructure.Data;

/// <summary>
/// Factory de diseño para que las herramientas de EF Core (ej. dotnet ef database update)
/// creen el DbContext sin arrancar la aplicación web, evitando fallos por Serilog/MySQL o JWT.
/// </summary>
public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    private const string DefaultConnection = "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

    public AdminDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? DefaultConnection;

        var optionsBuilder = new DbContextOptionsBuilder<AdminDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 0)),
            mysqlOptions =>
            {
                mysqlOptions.EnableStringComparisonTranslations();
                mysqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                mysqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Admin");
            });

        return new AdminDbContext(optionsBuilder.Options);
    }
}
