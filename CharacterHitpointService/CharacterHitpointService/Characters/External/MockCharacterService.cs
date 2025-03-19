using CharacterHitpointService.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterHitpointService.Characters.External;

public class MockCharacterService : ICharacterService
{
    public readonly CharacterDbContext _dbContext;

    public MockCharacterService(CharacterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Character?> GetCharacterAsync(string characterId)
    {
        // Simulate latency from an external service
        await Task.Delay(500);

        return await _dbContext.Characters
            .Include(c => c.Classes)
            .Include(c => c.Stats)
            .Include(c => c.Items)
            .Include(c => c.Defenses)
            .FirstOrDefaultAsync(c => c.Id == characterId);
    }
}