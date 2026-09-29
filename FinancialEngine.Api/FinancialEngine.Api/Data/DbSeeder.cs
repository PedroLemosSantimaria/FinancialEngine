using FinancialEngine.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialEngine.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Accounts.AnyAsync())
            return;

        context.Accounts.AddRange(
            new Account(Guid.Parse("7b895f64-5717-4562-b3fc-2c963f66afa7"), "Maria Silva", 1000m),
            new Account(Guid.NewGuid(), "João Souza", 250.50m),
            new Account(Guid.NewGuid(), "Ana Costa", 0m)
        );

        await context.SaveChangesAsync();
    }
}