using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Infrastructure.Database;
using OrgMembershipService.Infrastructure.Database.Seeding;

namespace OrgMembershipService.Api.Extensions;

internal static class MigrationExtensions
{
    internal static async Task ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    internal static async Task SeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seeder = new DbSeeder(dbContext);
        await seeder.SeedAsync();
    }
}