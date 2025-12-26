using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", true)
    .Build();

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole());
var connStr = config.GetConnectionString("DefaultConnection");
services.AddDbContext<ApplicationDbContext>(o => 
    o.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));
var sp = services.BuildServiceProvider();
var ctx = sp.GetRequiredService<ApplicationDbContext>();
var log = sp.GetRequiredService<ILogger<Program>>();

if (ctx.Database.CanConnect()) {
    ctx.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS __EFMigrationsHistory;");
    ctx.Database.EnsureCreated();
    log.LogInformation("Tablas creadas!");
} else {
    log.LogError("No se puede conectar");
}
