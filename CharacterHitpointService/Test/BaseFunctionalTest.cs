using CharacterHitpointService.Characters.External;
using CharacterHitpointService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Test;

public class BaseFunctionalTest : IClassFixture<TestWebAppFactory>
{
    public HttpClient HttpClient { get; init; }

    public BaseFunctionalTest(TestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HitpointsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();


    }
}