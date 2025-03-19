using CharacterHitpointService.Characters.External;
using CharacterHitpointService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Test;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<CharacterDbContext>));
            services.AddDbContext<CharacterDbContext>(options => options.UseSqlite("Data Source=test-characters.db"));

            services.RemoveAll(typeof(DbContextOptions<HitpointsDbContext>));
            services.AddDbContext<HitpointsDbContext>(options => options.UseSqlite("Data Source=test-hitpoints.db"));

            services.BuildServiceProvider();

            // var serviceProvider = services.BuildServiceProvider();
            // using var scope = serviceProvider.CreateScope();
            // var context = scope.ServiceProvider.GetRequiredService<HitpointsDbContext>();
            // context.Database.EnsureDeleted();
        });
    }
}