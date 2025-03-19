using CharacterHitpointService.Hitpoints.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterHitpointService.Infrastructure;

public class HitpointsDbContext : DbContext
{
    public HitpointsDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CharacterHitpointState> CharacterHealthStates { get; set; } = null!;
}