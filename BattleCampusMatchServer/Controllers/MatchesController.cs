using BattleCampus.Core;
using BattleCampus.MatchServer.Application.Matches.Commands;
using BattleCampus.MatchServer.Application.Matches.Query;
using BattleCampusMatchServer.Models;
using BattleCampusMatchServer.Models.DTOs;
using BattleCampusMatchServer.Services;
using MediatR;
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
        private readonly IMediator _mediator;

        //TODO : log all actions
        //TODO : 그냥 매치도 다 저장하고 여기서 매치매니저에 접속은 안하는게 나을듯? 매치매니저를 없앨까? 그러면 서버관리를 어케하지
        public MatchesController(IMatchManager matchManager, IMediator mediator)
        {
            _matchManager = matchManager;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<Match>>> GetAllMatchesAsync()
        {
            //HACK: don't convert to MatchDTO to reduce server load.
            return Ok(await _mediator.Send(new GetAll.Query()));
        }

        [HttpPost("create")]
        public async Task<ActionResult<MatchCreationResultDTO>> CreateMatchAsync([FromQuery] string name, [FromBody] GameUser user)
        {
            var result = await _mediator.Send(new CreateMatch.Command()
            {
                Name = name,
                User = user,
            });

            return Ok(result);
        }

        [HttpPost("join")]
        public async Task<ActionResult<MatchJoinResult>> JoinMatchAsync([FromQuery] string matchID,[FromBody]JoinMatch.Command command)
        {
            command.MatchID = matchID;
            var matchJoinResult = await _mediator.Send(command);
            return Ok(matchJoinResult);
        }

        //[HttpPost("notify/exit")]
        //public ActionResult NotifyPlayerExitMatch([FromQuery] string matchID, [FromQuery] string serverIp, [FromBody] GameUser user)
        //{
        //    _matchManager.NotifyPlayerExitGame(serverIp, matchID, user);

        //    return Ok();
        //}

        [HttpPost("notify/connect")]
        public async Task<ActionResult> NotifyUserConnectAsync([FromQuery] int connectionID, [FromBody] NotifyGameUserConnect.Command command)
        {
            command.User.ConnectionID = connectionID;

            await _mediator.Send(command);

            return Ok();
        }

        [Route("notify/exit")]
        [Route("notify/disconnet")]
        [HttpPost("notify/disconnect")]
        public ActionResult NotifyUserDisconnect([FromQuery] int connectionID, [FromBody] IpPortInfo serverIp)
        {
            _matchManager.DisconnectUser(serverIp, connectionID);

            return Ok();
        }
    }
}
