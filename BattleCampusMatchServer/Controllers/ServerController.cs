using BattleCampus.Core;
using BattleCampusMatchServer.Services;
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
        private readonly ILoggerFactory _loggerFactory;

        public ServerController(IMatchManager matchManager, ILoggerFactory loggerFactory)
        {
            _matchManager = matchManager;
            _loggerFactory = loggerFactory;
        }

        [Authorize]
        [HttpGet("all")]
        public ActionResult<List<GameServerModel>> GetServers()
        {
            throw new NotImplementedException();
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
