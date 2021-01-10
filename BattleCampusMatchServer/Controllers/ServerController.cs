using BattleCampus.Core;
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
    public class ServerController : ControllerBase
    {
        private readonly IMatchManager _matchManager;
        private readonly ILoggerFactory _loggerFactory;

        public ServerController(IMatchManager matchManager, ILoggerFactory loggerFactory)
        {
            _matchManager = matchManager;
            _loggerFactory = loggerFactory;
        }

        [HttpPost("register/{name}")]
        public ActionResult RegisterServer(string name, [FromQuery] int maxMatches, [FromBody] IpPortInfo ipPortInfo)
        {
            var server = new GameServer(name, ipPortInfo, _loggerFactory);
            server.MaxMatches = maxMatches;

            _matchManager.RegisterGameServer(server);

            return Ok();
        }

        [HttpDelete("unregister/{ipAddress}")]
        public void UnRegisterServer(string ipAddress)
        {
            _matchManager.UnRegisterGameServer(ipAddress);
        }
    }
}
