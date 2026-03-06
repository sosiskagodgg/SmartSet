using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessTracker.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        await ctx.Database.MigrateAsync();

        // TODO: add seeding data if necessary
    }
}
