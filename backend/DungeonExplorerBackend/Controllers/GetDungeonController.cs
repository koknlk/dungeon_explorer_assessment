using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DungeonExplorerBackend.Controllers
    {
    [Authorize(Policy = "CanGetDungeon")]
    [ApiController]
    [Route("api/v1/dungeons")]
    public class GetDungeonController : ControllerBase
        {
        private readonly IDungeonService _dungeonService;
        private readonly ILogger<GetDungeonController> _logger;
        private readonly IResponseHandler _responseHandler;

        public GetDungeonController(IDungeonService dungeonService, ILogger<GetDungeonController> logger,
                                   IResponseHandler responseHandler)
            {
            _dungeonService = dungeonService;
            _logger = logger;
            _responseHandler = responseHandler;
            }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DungeonResponse>>> GetDungeon(int id)
            {
            if (id <= 0)
                return _responseHandler.HandleError<DungeonResponse>("Invalid dungeon ID", 400);

            try
                {
                var dungeon = await _dungeonService.GetDungeonWithSolutionAsync(id);
                return _responseHandler.HandleSuccess(dungeon, "Dungeon retrieved successfully");
                }
            catch (Exception ex)
                {
                return _responseHandler.HandleException<DungeonResponse>(ex, _logger, $"retrieving dungeon {id}");
                }
            }
        }
    }