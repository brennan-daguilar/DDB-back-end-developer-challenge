using CharacterHitpointService.Api.Damage;
using CharacterHitpointService.CharacterService;
using CharacterHitpointService.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterHitpointService.Hitpoints;

public class HitpointService
{
    private readonly ICharacterService _characterService;
    private readonly HitpointsDbContext _dbContext;

    public HitpointService(ICharacterService characterService, HitpointsDbContext dbContext)
    {
        _characterService = characterService;
        _dbContext = dbContext;
    }

    public async Task<CharacterHealthState> GetCharacterHealthStateAsync(string characterId)
    {
        var healthState = await _dbContext.CharacterHealthStates
            .FirstOrDefaultAsync(s => s.CharacterId == characterId);

        if (healthState != null) return healthState;

        return await InitializeCharacterHealthStateAsync(characterId);
    }

    public async Task<CharacterHealthState> InitializeCharacterHealthStateAsync(string characterId)
    {
        var character = await _characterService.GetCharacterAsync(characterId);
        if (character == null)
        {
            throw new ArgumentException("Character not found.");
        }

        var healthState = new CharacterHealthState
        {
            CharacterId = characterId,
            Hitpoints = character.Hitpoints,
            TemporaryHitpoints = 0,
            MaxHitpoints = character.Hitpoints
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
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<DamageCharacterResponse> DamageCharacterAsync(string characterId, int damage,
        DamageType damageType)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage must be a positive number.");
        }
        
        var character = await _characterService.GetCharacterAsync(characterId);
        var health = await GetCharacterHealthStateAsync(characterId);
        if (character == null)
        {
            throw new ArgumentException("Character not found.");
        }

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

        var response = new DamageCharacterResponse()
        {
            CharacterId = characterId,
            Before = before,
            After = new CombinedHitpoints(health.Hitpoints, health.TemporaryHitpoints),
            TotalDamage = tempHpDamage + hpDamage,
            ResistanceEffect = resistanceEffect
        };

        await _dbContext.SaveChangesAsync();
        return response;
    }
}