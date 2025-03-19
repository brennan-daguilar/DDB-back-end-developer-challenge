using CharacterHitpointService.Characters;
using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Infrastructure;
using CharacterHitpointService.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Test.Hitpoints;

public class TestDatabaseFixture
{
    private const string ConnectionString = @"DataSource=test2.db";

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public TestDatabaseFixture()
    {
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContextWithoutTransation())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }

                _databaseInitialized = true;
            }
        }
    }

    public HitpointsDbContext CreateContextWithoutTransation() =>
        new(new DbContextOptionsBuilder<HitpointsDbContext>()
            .UseSqlite(ConnectionString)
            .Options);

    public HitpointsDbContext CreateContext()
    {
        var context = CreateContextWithoutTransation();
        context.Database.BeginTransaction();
        return context;
    }

    public LimitedCharacter BasicCharacter => new LimitedCharacter()
    {
        Hitpoints = 25,
        Defenses = new HashSet<CharacterDefense>()
        {
            new CharacterDefense()
            {
                Type = DamageType.Fire,
                Defense = DefenseValue.Immunity,
            },
            new CharacterDefense()
            {
                Type = DamageType.Slashing,
                Defense = DefenseValue.Resistance,
            }
        },
    };
}