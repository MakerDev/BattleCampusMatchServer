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
    public class MatchController : ControllerBase
    {
        private readonly IMatchManager _matchManager;

        public MatchController(IMatchManager matchManager)
        {
            _matchManager = matchManager;
        }

        [HttpGet]
        public ActionResult GetAllMatches()
        {
            return Ok();
        }

        [HttpPost("/trycreate/{name}")]
        public ActionResult<MatchCreationResultDTO> TryCreateMatch(string name)
        {
            var matchCreationResult = _matchManager.CreateNewMatch(name);

            if (matchCreationResult.IsCreationSuccess == false)
            {
                return BadRequest(new MatchCreationResultDTO
                {
                    IsCreationSuccess = false,
                    CreationFailReason = "Server already full",
                });
            }

            var matchDTO = new MatchDTO
            {
                MatchID = matchCreationResult.Match.MatchID,
                Name = matchCreationResult.Match.Name
            };

            var result = new MatchCreationResultDTO
            {
                IsCreationSuccess = true,
                Match = matchDTO
            };

            return Ok(result);
        }

        //TODO : implement this
        [HttpPut("/exit")]
        public ActionResult PlayerQuitGame(string playerID)
        {
            throw new NotImplementedException();
        }
    }
}
