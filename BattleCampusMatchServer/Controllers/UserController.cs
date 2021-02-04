using BattleCampus.Core;
using BattleCampus.MatchServer.Application.User;
using BattleCampus.MatchServer.Application.User.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AdminUser>> LoginAsAdminAsync([FromBody] Login.Query query)
        {
            var adminUser = await _mediator.Send(query);

            return Ok(adminUser);
        }

        [AllowAnonymous]
        [HttpPost("portal/login")]
        public async Task<ActionResult<bool>> LoginWithPortalAsync([FromBody] PortalLogin.Query query)
        {
            var result = await _mediator.Send(query);

            if (result)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(false);
            }
        }
    }
}
