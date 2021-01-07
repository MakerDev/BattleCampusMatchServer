using BattleCampusMatchServer.Models;
using BattleCampusMatchServer.Models.DTOs;
using BattleCampusMatchServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public MatchesController(IMatchManager matchManager)
        {
            _matchManager = matchManager;
        }

        [HttpGet]
        public ActionResult<List<Match>> GetAllMatches()
        {
            //HACK: don't convert to MatchDTO to reduce server load.
            return _matchManager.GetMatches();
        }

        [HttpPost("create")]
        public ActionResult<MatchCreationResultDTO> CreateMatch([FromQuery] string name)
        {
            var matchCreationResult = _matchManager.CreateNewMatch(name);

            if (matchCreationResult.IsCreationSuccess == false)
            {
                return BadRequest(new MatchCreationResultDTO
                {
                    IsCreationSuccess = false,
                    CreationFailReason = matchCreationResult.CreationFailReason,
                });
            }

            var matchDTO = new MatchDTO
            {
                MatchID = matchCreationResult.Match.MatchID,
                Name = matchCreationResult.Match.Name,
                CurrentPlayers = matchCreationResult.Match.CurrentPlayers,
                IpPortInfo = matchCreationResult.Match.IpPortInfo,
                MaxPlayers = matchCreationResult.Match.MaxPlayers,
            };

            var result = new MatchCreationResultDTO
            {
                IsCreationSuccess = true,
                Match = matchDTO,
            };

            return Ok(result);
        }



        //TODO : implement this
        [HttpPut("exit")]
        public ActionResult PlayerQuitGame(string playerID)
        {
            throw new NotImplementedException();
        }
    }
}
