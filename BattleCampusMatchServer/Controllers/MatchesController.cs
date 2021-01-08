using BattleCampusMatchServer.Models;
using BattleCampusMatchServer.Models.DTOs;
using BattleCampusMatchServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleCampusMatchServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchManager _matchManager;
        private readonly ILogger<MatchesController> _logger;

        //TODO : log all actions
        public MatchesController(IMatchManager matchManager, ILogger<MatchesController> logger)
        {
            _matchManager = matchManager;
            _logger = logger;

            _logger.LogInformation("Match Controller launched");
        }

        [HttpGet]
        public ActionResult<List<Match>> GetAllMatches()
        {
            //HACK: don't convert to MatchDTO to reduce server load.
            var matches = _matchManager.GetMatches();
            return matches;
        }

        [HttpPost("create")]
        public ActionResult<MatchCreationResultDTO> CreateMatch([FromQuery] string name, [FromBody] User user)
        {
            var matchCreationResult = _matchManager.CreateNewMatch(name, user);

            if (matchCreationResult.IsCreationSuccess == false)
            {
                return BadRequest(new MatchCreationResultDTO
                {
                    IsCreationSuccess = false,
                    CreationFailReason = matchCreationResult.CreationFailReason,
                });
            }

            var matchDTO = MatchDTO.CreateFromMatch(matchCreationResult.Match);

            var result = new MatchCreationResultDTO
            {
                IsCreationSuccess = true,
                Match = matchDTO,
            };

            return Ok(result);
        }

        [HttpPost("join")]
        public ActionResult<MatchJoinResult> JoinMatch([FromQuery] string serverIp, [FromQuery] string matchID, [FromBody] User user )
        {
            var joinResult = _matchManager.JoinMatch(serverIp, matchID, user);

            var matchJoinResult = new MatchJoinResultDTO
            {
                JoinFailReason = joinResult.JoinFailReason,
                JoinSucceeded = joinResult.JoinSucceeded,
                Match = MatchDTO.CreateFromMatch(joinResult.Match),
            };

            return Ok(matchJoinResult);
        }

        [HttpPost("notify/exit")]
        public ActionResult NotifyPlayerExitMatch([FromQuery] string serverIp, [FromQuery] string matchID, [FromBody] User user)
        {
            _matchManager.NotifyPlayerExitGame(serverIp, matchID, user);

            return Ok();
        }
    }
}
