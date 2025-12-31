using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BulkApi.Infrastructure;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = "Server=127.0.0.1;Port=3306;Database=bulkapi;User=app;Password=apppass;SslMode=None;Connection Timeout=5;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new AppDbContext(options);
    }
}
