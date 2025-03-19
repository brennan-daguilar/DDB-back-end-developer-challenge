using CharacterHitpointService.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterHitpointService.Characters.External;

public class CharacterDbContext : DbContext
{
    public DbSet<Character> Characters { get; set; } = null!;

    public CharacterDbContext(DbContextOptions<CharacterDbContext> options) : base(options)
    {
    }

}