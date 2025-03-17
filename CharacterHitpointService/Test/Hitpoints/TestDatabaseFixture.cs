using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Models;
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

    public Character BasicCharacter => new Character()
    {
        Name = "Briv",
        Level = 5,
        Hitpoints = 25,
        Classes = new HashSet<CharacterClass>
        {
            new CharacterClass()
            {
                Name = "fighter",
                HitDiceValue = 10,
                ClassLevel = 5
            }
        },
        Stats = new Stats()
        {
            Strength = 18,
            Dexterity = 14,
            Constitution = 16,
            Intelligence = 10,
            Wisdom = 12,
            Charisma = 8
        },
        Items = new HashSet<Item>
        {
            new Item()
            {
                Name = "Ioun Stone of Fortitude",
                Modifier = new ItemModifier()
                {
                    AffectedObject = "stats",
                    AffectedValue = "constitution",
                    Value = 2
                }
            }
        },
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