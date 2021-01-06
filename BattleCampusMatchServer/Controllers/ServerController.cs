using BattleCampusMatchServer.Models;
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
        [HttpPost("/register/{name}")]
        public ActionResult RegisterServer(string name, IpPortInfo ipPortInfo)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("/unregister/{ipAddress}")]
        public void UnRegisterServer(string ipAdress)
        {
            throw new NotImplementedException();
        }
    }
}
