using DungeonExplorerBackend.Models.AuthLayer;
using DungeonExplorerBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DungeonExplorerBackend.Data;

public class DungeonContext : DbContext
    {
    public DungeonContext(DbContextOptions<DungeonContext> options) : base(options)
        {
        }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Dungeon> Dungeons => Set<Dungeon>();
    public DbSet<DungeonSolution> Solutions => Set<DungeonSolution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        // Dungeon
        modelBuilder.Entity<Dungeon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");

            entity.OwnsOne(e => e.Start, o =>
            {
                o.Property(p => p.X).HasColumnName("Start_X").IsRequired();
                o.Property(p => p.Y).HasColumnName("Start_Y").IsRequired();
            });

            entity.OwnsOne(e => e.Goal, o =>
            {
                o.Property(p => p.X).HasColumnName("Goal_X").IsRequired();
                o.Property(p => p.Y).HasColumnName("Goal_Y").IsRequired();
            });

            entity.OwnsMany(e => e.Obstacles, o =>
            {
                o.WithOwner().HasForeignKey("DungeonId");
                o.HasKey("Id");
                o.Property<int>("Id").ValueGeneratedOnAdd();
                o.Property(p => p.X).IsRequired();
                o.Property(p => p.Y).IsRequired();
            });

            entity.HasMany(e => e.Solutions)
                  .WithOne(s => s.Dungeon)
                  .HasForeignKey(s => s.DungeonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DungeonSolution
        modelBuilder.Entity<DungeonSolution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SolvedAt).HasDefaultValueSql("datetime('now')");

            entity.OwnsMany(e => e.Path, o =>
            {
                o.WithOwner().HasForeignKey("DungeonSolutionId");
                o.HasKey("Id");
                o.Property<int>("Id").ValueGeneratedOnAdd();
                o.Property(p => p.X).IsRequired();
                o.Property(p => p.Y).IsRequired();
            });
        });

        // AppUser
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired();
        });
        }
    }