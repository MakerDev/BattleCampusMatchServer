using BattleCampus.Core;
using BattleCampus.MatchServer.Application.Server.Query;
using BattleCampusMatchServer.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public class ServerController : ControllerBase
    {
        private readonly IMatchManager _matchManager;
        private readonly IMediator _mediator;

        public ServerController(IMatchManager matchManager, IMediator mediator)
        {
            _matchManager = matchManager;
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("all")]
        public ActionResult<List<GameServerModel>> GetServers()
        {
            var servers = _matchManager.GetServers();
            var serverModels = new List<GameServerModel>();
            foreach (var server in servers)
            {
                serverModels.Add(new GameServerModel
                {
                    IpPortInfo = server.IpPortInfo,
                    MaxMatches = server.MaxMatches,
                    Name = server.Name,
                });
            }

            return Ok(serverModels);
        }

        [Authorize]
        [HttpGet("all/db")]
        public async Task<ActionResult<List<GameServerModel>>> GetServersFromDbAsync()
        {
            return Ok(await _mediator.Send(new GetAllFromDb.Query()));
        }


        [Authorize]
        [HttpGet]
        public ActionResult<GameServerModel> GetServer(IpPortInfo ipPortInfo)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        [HttpPost("add")]
        public ActionResult AddServer(GameServerModel gameServer)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        [HttpPut("rename")]
        public ActionResult RenameServer(string name, IpPortInfo ipPortInfo)
        {
            throw new NotImplementedException();
        }

        [HttpPost("register/{name}")]
        public async Task<ActionResult> RegisterServerAsync(string name, [FromQuery] int maxMatches, [FromBody] IpPortInfo ipPortInfo)
        {
            await _matchManager.RegisterGameServerAsync(name, maxMatches, ipPortInfo);

            return Ok();
        }

        [HttpDelete("turnoff")]
        public async Task TurnOffServerAsync(IpPortInfo ipAddress)
        {
            await _matchManager.TurnOffServerAsync(ipAddress);
        }

        [Authorize]
        [HttpDelete("unregister")]
        public async Task UnregisterServerAsync(IpPortInfo ipAddress)
        {
            await _matchManager.UnRegisterGameServerAsync(ipAddress);
        }
    }
}
