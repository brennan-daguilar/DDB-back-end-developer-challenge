using CharacterHitpointService.Characters;
using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Hitpoints.Models;
using CharacterHitpointService.Shared.Models;
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

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.InitializeCharacterHealthStateAsync("briv");
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");


        // Assert
        result.ShouldNotBeNull();
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(25);
        charStatus.TemporaryHitpoints.ShouldBe(0);
    }

    [Fact]
    public async Task InitializeCharacterHealthStateAsync_WithUnknownCharacter_ShouldFail()
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync((LimitedCharacter?)null);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.InitializeCharacterHealthStateAsync("briv");
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");


        // Assert
        result.ShouldBeNull();
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
        dbContext.CharacterHealthStates.Add(new CharacterHitpointState()
        {
            CharacterId = "briv",
            Hitpoints = 25,
            TemporaryHitpoints = 10,
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", damage, DamageType.Acid);
        dbContext.ChangeTracker.Clear();
        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");

        // Assert
        result.ShouldNotBeNull();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.CharacterId.ShouldBe("briv");
        response.Before.ShouldBe(new CombinedHitpoints(25, 10));
        response.After.ShouldBe(new CombinedHitpoints(expectedHp, expectedTempHp));
        response.TotalDamage.ShouldBe(expectedTotalDamage);
        response.ResistanceEffect.ShouldBeNull();
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
        dbContext.CharacterHealthStates.Add(new CharacterHitpointState()
        {
            CharacterId = "briv",
            Hitpoints = 25,
            TemporaryHitpoints = 10,
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", damage, DamageType.Slashing);
        dbContext.ChangeTracker.Clear();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.CharacterId.ShouldBe("briv");
        response.Before.ShouldBe(new CombinedHitpoints(25, 10));
        response.After.ShouldBe(new CombinedHitpoints(expectedHp, expectedTempHp));
        response.TotalDamage.ShouldBe(expectedTotalDamage);
        response.ResistanceEffect.ShouldBe(DamageResistanceEffect.Resisted);

        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");
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
        dbContext.CharacterHealthStates.Add(new CharacterHitpointState()
        {
            CharacterId = "briv",
            Hitpoints = 25,
            TemporaryHitpoints = 10,
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", damage, DamageType.Fire);
        dbContext.ChangeTracker.Clear();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.CharacterId.ShouldBe("briv");
        response.Before.ShouldBe(new CombinedHitpoints(25, 10));
        response.After.ShouldBe(new CombinedHitpoints(25, 10));
        response.TotalDamage.ShouldBe(0);
        response.ResistanceEffect.ShouldBe(DamageResistanceEffect.Immune);

        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(25);
        charStatus.TemporaryHitpoints.ShouldBe(10);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task DamageCharacterAsync_WithNegativeDamage_ShouldFail(int damage)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", damage, DamageType.Fire);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Damage must be a positive number.");
    }

    [Fact]
    public async Task DamageCharacterAsync_WithUnknownCharacter_ShouldFail()
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync(It.IsAny<string>()))
            .ReturnsAsync((LimitedCharacter?)null);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.DamageCharacterAsync("briv", 10, DamageType.Cold);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Character not found.");
    }

    [Theory]
    [InlineData(0, 25, 25, 0)]
    [InlineData(5, 25, 25, 0)]
    [InlineData(10, 0, 10, 10)]
    [InlineData(25, 0, 25, 25)]
    [InlineData(30, 0, 25, 25)]
    [InlineData(int.MaxValue, 0, 25, 25)]
    [InlineData(int.MaxValue, 25, 25, 0)]
    public async Task HealCharacterAsync(int healAmount, int initialHp, int expectedHp, int expectedHealAmount)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        dbContext.CharacterHealthStates.Add(new CharacterHitpointState()
        {
            CharacterId = "briv",
            Hitpoints = initialHp,
            TemporaryHitpoints = 0,
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.HealCharacterAsync("briv", healAmount);
        dbContext.ChangeTracker.Clear();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.CharacterId.ShouldBe("briv");
        response.Before.ShouldBe(new CombinedHitpoints(initialHp, 0));
        response.After.ShouldBe(new CombinedHitpoints(expectedHp, 0));
        response.ActualHealed.ShouldBe(expectedHealAmount);

        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(expectedHp);
        charStatus.TemporaryHitpoints.ShouldBe(0);
    }


    [Fact]
    public async Task HealCharacterAsync_WithUnknownCharacter_ShouldReturnError()
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync((LimitedCharacter?)null);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.HealCharacterAsync("briv", 10);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Character not found.");
    }


    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task HealCharacterAsync_WithNegativeAmount_ShouldReturnError(int healAmount)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        dbContext.CharacterHealthStates.Add(new CharacterHitpointState()
        {
            CharacterId = "briv",
            Hitpoints = 15,
            TemporaryHitpoints = 0,
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.HealCharacterAsync("briv", healAmount);
        dbContext.ChangeTracker.Clear();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Healing amount must be a positive number.");

        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(15);
        charStatus.TemporaryHitpoints.ShouldBe(0);
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(0, 10, 10, 10)]
    [InlineData(5, 10, 10, 5)]
    [InlineData(10, 5, 10, 0)]
    [InlineData(10, 10, 10, 0)]
    public async Task AddTemporaryHitpointsAsync(int initialTempHp, int addedTempHp, int expectedTempHp,
        int expectedGained)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        dbContext.CharacterHealthStates.Add(new CharacterHitpointState()
        {
            CharacterId = "briv",
            Hitpoints = 25,
            TemporaryHitpoints = initialTempHp,
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.AddTemporaryHitpointsAsync("briv", addedTempHp);
        dbContext.ChangeTracker.Clear();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value;
        response.ShouldNotBeNull();
        response.CharacterId.ShouldBe("briv");
        response.Before.ShouldBe(new CombinedHitpoints(25, initialTempHp));
        response.After.ShouldBe(new CombinedHitpoints(25, expectedTempHp));
        response.Gained.ShouldBe(expectedGained);

        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(25);
        charStatus.TemporaryHitpoints.ShouldBe(expectedTempHp);
    }

    [Fact]
    public async Task AddTemporaryHitpointsAsync_WithUnknownCharacter_ShouldReturnError()
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync((LimitedCharacter?)null);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.AddTemporaryHitpointsAsync("briv", 10);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Failed to retrieve or create character health state.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task AddTemporaryHitpointsAsync_WithNegativeAmount_ShouldReturnError(int tempHp)
    {
        // Arrange
        await using var dbContext = Fixture.CreateContext();
        dbContext.CharacterHealthStates.Add(new CharacterHitpointState()
        {
            CharacterId = "briv",
            Hitpoints = 15,
            TemporaryHitpoints = 0,
        });
        await dbContext.SaveChangesAsync();

        var characterService = new Mock<ICharacterRepository>();
        characterService.Setup(cs => cs.GetCharacterAsync("briv"))
            .ReturnsAsync(Fixture.BasicCharacter);

        var service = new HitpointService(characterService.Object, dbContext);

        // Act
        var result = await service.AddTemporaryHitpointsAsync("briv", tempHp);
        dbContext.ChangeTracker.Clear();

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Temporary hitpoints must be a positive number.");

        var charStatus = await dbContext.CharacterHealthStates.FindAsync("briv");
        charStatus.ShouldNotBeNull();
        charStatus.Hitpoints.ShouldBe(15);
        charStatus.TemporaryHitpoints.ShouldBe(0);
    }
}