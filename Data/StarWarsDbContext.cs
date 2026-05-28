using Microsoft.EntityFrameworkCore;
using StarWarsApi.Models;

namespace StarWarsApi.Data;

public class StarWarsDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Planet> Planets { get; set; }
    public DbSet<FavoriteCharacter> FavoriteCharacters { get; set; }
    public DbSet<FavoritePlanet> FavoritePlanets { get; set; }

    public StarWarsDbContext(DbContextOptions<StarWarsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .HasMaxLength(500)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoriteCharacters)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.FavoritePlanets)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<FavoriteCharacter>()
            .HasKey(f => new { f.UserId, f.CharacterId });

        modelBuilder.Entity<FavoritePlanet>()
            .HasKey(f => new { f.UserId, f.PlanetId });

        modelBuilder.Entity<Character>()
            .Property(c => c.Name)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Character>()
            .Property(c => c.Species)
            .HasMaxLength(100);

        modelBuilder.Entity<Character>()
            .Property(c => c.Gender)
            .HasMaxLength(50);

        modelBuilder.Entity<Planet>()
            .Property(p => p.Name)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Planet>()
            .Property(p => p.Climate)
            .HasMaxLength(100);

        modelBuilder.Entity<Planet>()
            .Property(p => p.Terrain)
            .HasMaxLength(100);
    }
}
