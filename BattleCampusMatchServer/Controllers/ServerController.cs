using BattleCampusMatchServer.Models;
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
    public class ServerController : ControllerBase
    {
        private readonly IMatchManager _matchManager;

        public ServerController(IMatchManager matchManager)
        {
            _matchManager = matchManager;
        }

        [HttpPost("register/{name}")]
        public ActionResult RegisterServer(string name, [FromQuery] int maxMatches, [FromBody] IpPortInfo ipPortInfo)
        {
            var server = new GameServer(name, ipPortInfo);
            server.MaxMatches = maxMatches;

            _matchManager.RegisterGameServer(server);

            return Ok();
        }

        [HttpDelete("unregister/{ipAddress}")]
        public void UnRegisterServer(string ipAdress)
        {
            _matchManager.UnRegisterGameServer(ipAdress);
        }
    }
}
