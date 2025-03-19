using CharacterHitpointService.Api.AddTemporaryHitpoints;
using CharacterHitpointService.Api.Damage;
using CharacterHitpointService.Api.Heal;
using CharacterHitpointService.Characters;
using CharacterHitpointService.Hitpoints.Models;
using CharacterHitpointService.Infrastructure;
using CharacterHitpointService.Shared.Models;
using CharacterHitpointService.Util;
using Microsoft.EntityFrameworkCore;

namespace CharacterHitpointService.Hitpoints;

public class HitpointService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly HitpointsDbContext _dbContext;

    public HitpointService(ICharacterRepository characterRepository, HitpointsDbContext dbContext)
    {
        _characterRepository = characterRepository;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the current health state of a character or initialize it if it doesn't exist.
    /// </summary>
    /// <param name="characterId">The character's id</param>
    /// <returns>The character's health state unless an error occurs</returns>
    public async Task<CharacterHitpointState?> GetOrCreateCharacterHealthStateAsync(string characterId)
    {
        var healthState = await _dbContext.CharacterHealthStates
            .FirstOrDefaultAsync(s => s.CharacterId == characterId);

        if (healthState is not null) return healthState;

        return await InitializeCharacterHealthStateAsync(characterId);
    }

    /// <summary>
    /// Initializes a character's health state in the database.
    /// When a character is unknown to the hitpoint service, this creates a starting state for the character's hitpoints
    /// based on the character's base hitpoints.
    /// </summary>
    /// <param name="characterId">The character's id</param>
    /// <returns>The newly initialized health state or null if the character details can't be loaded</returns>
    public async Task<CharacterHitpointState?> InitializeCharacterHealthStateAsync(string characterId)
    {
        var character = await _characterRepository.GetCharacterAsync(characterId);
        if (character is null)
            return null;

        var healthState = new CharacterHitpointState
        {
            CharacterId = characterId,
            Hitpoints = character.Hitpoints,
            TemporaryHitpoints = 0,
        };

        _dbContext.CharacterHealthStates.Add(healthState);
        await _dbContext.SaveChangesAsync();

        return healthState;
    }


    /// <summary>
    /// Deal damage to a character.
    /// Damage is affected by the characters resistance to the damage type. Resistance will reduce damage by half,
    /// immunity will prevent all damage.
    /// Damage is always applied to temporary hitpoints first, then to hitpoints.
    /// </summary>
    /// <param name="characterId">The character's id</param>
    /// <param name="damage">The amount of damage the attack should do before resistance</param>
    /// <param name="damageType">The damage type of the attack</param>
    /// <returns>A Result object with a DamageCharacterResponse when successful, otherwise an error</returns>
    public async Task<Result<DamageCharacterResult>> DamageCharacterAsync(string characterId, int damage,
        DamageType damageType)
    {
        if (damage < 0)
            return Result<DamageCharacterResult>.Failure("Damage must be a positive number.");

        var character = await _characterRepository.GetCharacterAsync(characterId);
        if (character is null)
            return Result<DamageCharacterResult>.Failure("Character not found.");

        var health = await GetOrCreateCharacterHealthStateAsync(characterId);
        if (health is null)
            return Result<DamageCharacterResult>.Failure("Failed to retrieve or create character health state.");

        var before = new CombinedHitpoints(health.Hitpoints, health.TemporaryHitpoints);

        // Characters could have multiple defenses of the same type, only use the strongest 
        var defense = character.Defenses
            .Where(d => d.Type == damageType)
            .OrderByDescending(d => d.Defense)
            .Select(d => d.Defense)
            .FirstOrDefault();

        // Calculate modified damage based on defense
        int modifiedDamage;
        DamageResistanceEffect? resistanceEffect;

        switch (defense)
        {
            case DefenseValue.Immunity:
                modifiedDamage = 0;
                resistanceEffect = DamageResistanceEffect.Immune;
                break;
            case DefenseValue.Resistance:
                modifiedDamage = damage / 2;
                resistanceEffect = DamageResistanceEffect.Resisted;
                break;
            default:
                modifiedDamage = damage;
                resistanceEffect = null;
                break;
        }

        // Apply damage to temporary hitpoints first
        var tempHpDamage = Math.Min(health.TemporaryHitpoints, modifiedDamage);
        health.TemporaryHitpoints -= tempHpDamage;

        // Apply remaining damage to hitpoints
        var hpDamage = Math.Min(health.Hitpoints, modifiedDamage - tempHpDamage);
        health.Hitpoints -= hpDamage;

        await _dbContext.SaveChangesAsync();

        return Result<DamageCharacterResult>.Success(new DamageCharacterResult()
        {
            CharacterId = characterId,
            Before = before,
            After = new CombinedHitpoints(health.Hitpoints, health.TemporaryHitpoints),
            TotalDamage = tempHpDamage + hpDamage,
            ResistanceEffect = resistanceEffect
        });
    }

    /// <summary>
    /// Heal a character by a given amount.
    /// Healing is applied up to the character's max hitpoints. 
    /// </summary>
    /// <param name="characterId">The character's id</param>
    /// <param name="amount">The amount of hitpoints to heal</param>
    /// <returns>A Result object with a HealCharacterResponse when successful, otherwise an error</returns>
    public async Task<Result<HealCharacterResult>> HealCharacterAsync(string characterId, int amount)
    {
        if (amount < 0)
            return Result<HealCharacterResult>.Failure("Healing amount must be a positive number.");

        var character = await _characterRepository.GetCharacterAsync(characterId);
        if (character is null)
            return Result<HealCharacterResult>.Failure("Character not found.");
        
        var health = await GetOrCreateCharacterHealthStateAsync(characterId);
        if (health is null)
            return Result<HealCharacterResult>.Failure("Failed to retrieve or create character health state.");

        var before = new CombinedHitpoints(health.Hitpoints, health.TemporaryHitpoints);

        var actualHealAmount = Math.Min(amount, character.Hitpoints - health.Hitpoints);
        health.Hitpoints += actualHealAmount;
        await _dbContext.SaveChangesAsync();

        return Result<HealCharacterResult>.Success(new HealCharacterResult()
        {
            CharacterId = characterId,
            Before = before,
            After = new CombinedHitpoints(health.Hitpoints, health.TemporaryHitpoints),
            ActualHealed = health.Hitpoints - before.Hitpoints
        });
    }


    /// <summary>
    /// Add temporary hitpoints to a character.
    /// The character's temporary hitpoints will be set to the greater of the current temporary hitpoints or the given amount.
    /// </summary>
    /// <param name="characterId">The character's id</param>
    /// <param name="amount">The number of temporary hitpoints to add to the character</param>
    /// <returns>A Result object with a AddTemporaryHitpointsResponse when successful, otherwise an error</returns>
    public async Task<Result<AddTemporaryHitpointsResult>> AddTemporaryHitpointsAsync(string characterId, int amount)
    {
        if (amount < 0)
            return Result<AddTemporaryHitpointsResult>.Failure("Temporary hitpoints must be a positive number.");

        var health = await GetOrCreateCharacterHealthStateAsync(characterId);
        if (health is null)
            return Result<AddTemporaryHitpointsResult>.Failure(
                "Failed to retrieve or create character health state.");

        var before = new CombinedHitpoints(health.Hitpoints, health.TemporaryHitpoints);

        health.TemporaryHitpoints = Math.Max(amount, health.TemporaryHitpoints);
        await _dbContext.SaveChangesAsync();

        return Result<AddTemporaryHitpointsResult>.Success(new AddTemporaryHitpointsResult()
        {
            CharacterId = characterId,
            Before = before,
            After = new CombinedHitpoints(health.Hitpoints, health.TemporaryHitpoints),
            Gained = health.TemporaryHitpoints - before.TemporaryHitpoints
        });
    }
}