using Microsoft.EntityFrameworkCore;

namespace CharacterHitpointService.Hitpoints;

public class HitpointsDbContext : DbContext
{
    public DbSet<CharacterHealthState> CharacterHealthStates { get; set; } = null!;
}