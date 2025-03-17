using CharacterHitpointService.CharacterService;
using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Models;
using Moq;
using Shouldly;

namespace Test.Hitpoints;

public class HitpointServiceTest : IClassFixture<TestDatabaseFixture>
{
    private HitpointService _service;

    public HitpointServiceTest(TestDatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public TestDatabaseFixture Fixture { get; }

    [Fact]
    public async Task InitializeCharacterHealthStateAsync_ShouldAddToDatabase()
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();

        var characterService = new Mock<ICharacterService>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);


        // Act
        await service.InitializeCharacterHealthStateAsync("briv");
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");


        // Assert
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(25);
        charStatus.TemporaryHitpoints.ShouldBe(0);
    }

    [Fact]
    public async Task InitializeCharacterHealthStateAsync_WithUnknownCharacter_ShouldThrowException()
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();

        var characterService = new Mock<ICharacterService>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync((Character?)null);

        var service = new HitpointService(characterService.Object, dbContext);


        // Act
        await Should.ThrowAsync<ArgumentException>(async () =>
            await service.InitializeCharacterHealthStateAsync("briv")
        );
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");


        // Assert
        charStatus.ShouldBeNull();
    }

    [Theory]
    [InlineData(0, 25, 10, 0)]
    [InlineData(5, 25, 5, 5)]
    [InlineData(10, 25, 0, 10)]
    [InlineData(11, 24, 0, 11)]
    [InlineData(15, 20, 0, 15)]
    [InlineData(25, 10, 0, 25)]
    [InlineData(35, 0, 0, 35)]
    [InlineData(36, 0, 0, 35)]
    [InlineData(int.MaxValue, 0, 0, 35)]
    public async Task DamageCharacterAsync_NormalDamage(int damage, int expectedHp, int expectedTempHp,
        int expectedTotalDamage)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        dbContext.CharacterHealthStates.Add(new CharacterHealthState()
        {
            CharacterId = "briv",
            Hitpoints = 25,
            TemporaryHitpoints = 10,
            MaxHitpoints = 25
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterService>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", damage, DamageType.Acid);
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");

        // Assert
        result.CharacterId.ShouldBe("briv");
        result.Before.ShouldBe(new CombinedHitpoints(25, 10));
        result.After.ShouldBe(new CombinedHitpoints(expectedHp, expectedTempHp));
        result.TotalDamage.ShouldBe(expectedTotalDamage);
        result.ResistanceEffect.ShouldBeNull();
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(expectedHp);
        charStatus.TemporaryHitpoints.ShouldBe(expectedTempHp);
    }

    [Theory]
    [InlineData(0, 25, 10, 0)]
    [InlineData(5, 25, 8, 2)]
    [InlineData(10, 25, 5, 5)]
    [InlineData(20, 25, 0, 10)]
    [InlineData(22, 24, 0, 11)]
    [InlineData(68, 1, 0, 34)]
    [InlineData(70, 0, 0, 35)]
    [InlineData(72, 0, 0, 35)]
    [InlineData(int.MaxValue, 0, 0, 35)]
    public async Task DamageCharacterAsync_ResistantDamage(int damage, int expectedHp, int expectedTempHp,
        int expectedTotalDamage)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        dbContext.CharacterHealthStates.Add(new CharacterHealthState()
        {
            CharacterId = "briv",
            Hitpoints = 25,
            TemporaryHitpoints = 10,
            MaxHitpoints = 25
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterService>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", damage, DamageType.Slashing);
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");

        // Assert
        result.CharacterId.ShouldBe("briv");
        result.Before.ShouldBe(new CombinedHitpoints(25, 10));
        result.After.ShouldBe(new CombinedHitpoints(expectedHp, expectedTempHp));
        result.TotalDamage.ShouldBe(expectedTotalDamage);
        result.ResistanceEffect.ShouldBe(DamageResistanceEffect.Resisted);
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(expectedHp);
        charStatus.TemporaryHitpoints.ShouldBe(expectedTempHp);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public async Task DamageCharacterAsync_ImmuneDamage(int damage)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        dbContext.CharacterHealthStates.Add(new CharacterHealthState()
        {
            CharacterId = "briv",
            Hitpoints = 25,
            TemporaryHitpoints = 10,
            MaxHitpoints = 25
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterService>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", damage, DamageType.Fire);
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");

        // Assert
        result.CharacterId.ShouldBe("briv");
        result.Before.ShouldBe(new CombinedHitpoints(25, 10));
        result.After.ShouldBe(new CombinedHitpoints(25, 10));
        result.TotalDamage.ShouldBe(0);
        result.ResistanceEffect.ShouldBe(DamageResistanceEffect.Immune);
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(25);
        charStatus.TemporaryHitpoints.ShouldBe(10);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task DamageCharacterAsync_WithNegativeDamage_ShouldThrowException(int damage)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();

        var characterService = new Mock<ICharacterService>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act & Assert
        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            await service.DamageCharacterAsync("briv", damage, DamageType.Fire));
    }

    [Fact]
    public async Task DamageCharacterAsync_WithUnknownCharacter_ShouldThrowException()
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();

        var characterService = new Mock<ICharacterService>();
        characterService.Setup(cs => cs.GetCharacterAsync(It.IsAny<string>()))
            .ReturnsAsync((Character?)null);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await service.DamageCharacterAsync("briv", 10, DamageType.Cold));
    }
}