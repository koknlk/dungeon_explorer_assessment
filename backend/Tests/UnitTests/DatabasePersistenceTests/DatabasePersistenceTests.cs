using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.UnitTests.DatabasePersistenceTests
    {
    public class DatabasePersistenceTests
        {
        private DungeonContext CreateContext()
            {
            var options = new DbContextOptionsBuilder<DungeonContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new DungeonContext(options);
            }

        [Fact]
        public async Task DungeonContext_SaveAndRetrieve_WorksCorrectly()
            {
            using var context = CreateContext();

            var dungeon = new Dungeon
                {
                Name = "Persistence Test",
                Width = 10,
                Height = 10,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 9, Y = 9 },
                Obstacles = new List<Position>
                {
                    new Position { X = 1, Y = 1 },
                    new Position { X = 2, Y = 2 }
                }
                };

            context.Dungeons.Add(dungeon);
            await context.SaveChangesAsync();

            var retrieved = await context.Dungeons
                .Include(d => d.Obstacles)
                .FirstOrDefaultAsync(d => d.Id == dungeon.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(dungeon.Name, retrieved.Name);
            Assert.Equal(2, retrieved.Obstacles.Count);
            Assert.Equal(dungeon.Start.X, retrieved.Start.X);
            Assert.Equal(dungeon.Start.Y, retrieved.Start.Y);
            }

        [Fact]
        public async Task DungeonContext_CascadeDelete_SolutionsRemoved()
            {
            using var context = CreateContext();

            var dungeon = new Dungeon
                {
                Name = "Cascade Test",
                Width = 5,
                Height = 5,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 4, Y = 4 },
                Solutions = new List<DungeonSolution>
                {
                    new DungeonSolution
                    {
                        Path = new List<Position>
                        {
                            new Position { X = 0, Y = 0 },
                            new Position { X = 4, Y = 4 }
                        }
                    }
                }
                };

            context.Dungeons.Add(dungeon);
            await context.SaveChangesAsync();

            var solutionCountBefore = await context.Solutions.CountAsync();
            context.Dungeons.Remove(dungeon);
            await context.SaveChangesAsync();
            var solutionCountAfter = await context.Solutions.CountAsync();

            Assert.Equal(1, solutionCountBefore);
            Assert.Equal(0, solutionCountAfter);
            }

        [Fact]
        public async Task DungeonContext_OwnedTypes_PersistCorrectly()
            {
            using var context = CreateContext();

            var dungeon = new Dungeon
                {
                Name = "Owned Types Test",
                Width = 3,
                Height = 3,
                Start = new Position { X = 1, Y = 1 },
                Goal = new Position { X = 2, Y = 2 },
                Obstacles = new List<Position>
                {
                    new Position { X = 0, Y = 0 },
                    new Position { X = 0, Y = 1 }
                }
                };

            context.Dungeons.Add(dungeon);
            await context.SaveChangesAsync();

            var retrieved = await context.Dungeons
                .AsNoTracking()
                .Include(d => d.Obstacles)
                .FirstOrDefaultAsync(d => d.Id == dungeon.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(1, retrieved.Start.X);
            Assert.Equal(1, retrieved.Start.Y);
            Assert.Equal(2, retrieved.Obstacles.Count);
            Assert.Contains(retrieved.Obstacles, o => o.X == 0 && o.Y == 0);
            Assert.Contains(retrieved.Obstacles, o => o.X == 0 && o.Y == 1);
            }

        [Fact]
        public async Task DungeonContext_MultipleDungeons_IsolateCorrectly()
            {
            using var context = CreateContext();

            var dungeon1 = new Dungeon
                {
                Name = "First",
                Width = 5,
                Height = 5,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 4, Y = 4 }
                };

            var dungeon2 = new Dungeon
                {
                Name = "Second",
                Width = 5,
                Height = 5,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 4, Y = 4 }
                };

            context.Dungeons.AddRange(dungeon1, dungeon2);
            await context.SaveChangesAsync();

            var allDungeons = await context.Dungeons.ToListAsync();

            Assert.Equal(2, allDungeons.Count);
            Assert.Contains(allDungeons, d => d.Name == "First");
            Assert.Contains(allDungeons, d => d.Name == "Second");
            }
        }
    }