using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DungeonExplorerBackend.Controllers
    {
    [Authorize(Policy = "CanCreateDungeon")]
    [ApiController]
    [Route("api/v1/dungeons/create")]
    public class CreateDungeonController : ControllerBase
        {
        private readonly IDungeonService _dungeonService;
        private readonly IInputSanitizer _sanitizer;
        private readonly ILogger<CreateDungeonController> _logger;
        private readonly IResponseHandler _responseHandler;

        public CreateDungeonController(IDungeonService dungeonService, IInputSanitizer sanitizer,
                                     ILogger<CreateDungeonController> logger, IResponseHandler responseHandler)
            {
            _dungeonService = dungeonService;
            _sanitizer = sanitizer;
            _logger = logger;
            _responseHandler = responseHandler;
            }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateDungeon([FromBody] CreateDungeonRequest request)
            {
            try
                {
                request.Name = _sanitizer.Sanitize(request.Name);
                var id = await _dungeonService.CreateDungeonAsync(request);

                return _responseHandler.HandleSuccess(id, "Dungeon created successfully");
                }
            catch (Exception ex)
                {
                return _responseHandler.HandleException<int>(ex, _logger, "dungeon creation");
                }
            }
        }
    }