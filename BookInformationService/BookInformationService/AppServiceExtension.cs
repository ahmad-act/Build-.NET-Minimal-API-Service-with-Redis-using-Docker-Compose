using Microsoft.EntityFrameworkCore;
using BookInformationService.DatabaseContext;
using Serilog;

namespace BookInformationService;

public static class AppServiceExtension
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope serviceScope = app.ApplicationServices.CreateScope();

        using SystemDbContext dbContext = serviceScope.ServiceProvider.GetRequiredService<SystemDbContext>();

        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            Log.Verbose(ex, ex.Message);
        }
    }

}

