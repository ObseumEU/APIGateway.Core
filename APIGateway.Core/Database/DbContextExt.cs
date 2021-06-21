using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace APIGateway.Core.Database
{
    public class DbContextExt : DbContext
    {
        private readonly ILogger _log;

        public DbContextExt(DbContextOptions options,
            ILogger log) : base(options)
        {
            _log = log;
        }

        public async Task MigrateAsync()
        {
            //Run migration
            var pendingMigrations = await Database.GetPendingMigrationsAsync();
            _log.LogInformation("Pending database migrations:" + string.Join(",", pendingMigrations.Select(m => m)));
            if (pendingMigrations.Any())
            {
                _log.LogInformation("Database migrations start: " + string.Join(",", pendingMigrations.Select(m => m)));
                await Database.MigrateAsync();
            }

            await SaveChangesAsync();
            _log.LogInformation("Database migrations: OK " + string.Join(",", pendingMigrations.Select(m => m)));
        }
    }
}