using DungeonExplorerBackend.Models.Requests;
using DungeonExplorerBackend.Models.Responses;

namespace DungeonExplorerBackend.Contracts
    {
    public interface IDungeonService
        {
        Task<int> CreateDungeonAsync(CreateDungeonRequest request);

        Task<DungeonResponse> GetDungeonWithSolutionAsync(int id);
        }
    }