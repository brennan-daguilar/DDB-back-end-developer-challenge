using Microsoft.EntityFrameworkCore;

namespace CharacterHitpointService.Hitpoints;

public class HitpointsDbContext : DbContext
{
    public HitpointsDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CharacterHealthState> CharacterHealthStates { get; set; } = null!;
}