using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.Entities;
using DungeonExplorerBackend.Models.Requests;
using DungeonExplorerBackend.Models.Responses;

namespace DungeonExplorerBackend.Services
    {
    public class DungeonService : IDungeonService
        {
        private readonly IDungeonRepository _dungeonRepo;
        private readonly DungeonSolverService _solverService;
        private readonly DungeonMapper _mapper;
        private readonly ILogger<DungeonService> _logger;

        public DungeonService(
            IDungeonRepository dungeonRepo,
            DungeonSolverService solverService,
            DungeonMapper mapper,
            ILogger<DungeonService> logger)
            {
            _dungeonRepo = dungeonRepo;
            _solverService = solverService;
            _mapper = mapper;
            _logger = logger;
            }

        public async Task<int> CreateDungeonAsync(CreateDungeonRequest request)
            {
            // Optional: validate width/height/start/goal/obstacles
            if (request.Width < 5 || request.Width > 50 || request.Height < 5 || request.Height > 50)
                throw new ArgumentException("Dungeon width/height must be between 5 and 50.");

            var dungeon = new Dungeon
                {
                Name = request.Name,
                Width = request.Width,
                Height = request.Height,
                Start = new Position { X = request.Start.X, Y = request.Start.Y },
                Goal = new Position { X = request.Goal.X, Y = request.Goal.Y },
                Obstacles = request.Obstacles.Select(o => new Position { X = o.X, Y = o.Y }).ToList()
                };

            await _dungeonRepo.AddAsync(dungeon);
            await _dungeonRepo.SaveChangesAsync();

            _logger.LogInformation("Created dungeon {id} with {obstacles} obstacles", dungeon.Id, dungeon.Obstacles.Count);

            return dungeon.Id;
            }

        public async Task<DungeonResponse> GetDungeonWithSolutionAsync(int id)
            {
            var dungeon = await _dungeonRepo.GetByIdAsync(id);
            if (dungeon == null)
                {
                _logger.LogWarning("Dungeon {id} not found", id);
                throw new KeyNotFoundException($"Dungeon with ID {id} not found");
                }

            dungeon.Solutions ??= new List<DungeonSolution>();

            var solution = dungeon.Solutions.OrderByDescending(s => s.SolvedAt).FirstOrDefault();

            if (solution == null)
                {
                _logger.LogInformation("Computing path for dungeon {id}", id);
                var (path, computationTime) = _solverService.ComputePath(dungeon);

                solution = new DungeonSolution
                    {
                    Path = path,
                    ComputationTimeMs = computationTime,
                    SolvedAt = DateTime.UtcNow
                    };

                dungeon.Solutions.Add(solution);
                await _dungeonRepo.SaveChangesAsync();
                }

            return _mapper.MapToResponse(dungeon, solution);
            }
        }
    }