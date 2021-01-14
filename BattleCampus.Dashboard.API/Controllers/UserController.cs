﻿using BattleCampus.Dashboard.Application.User;
using BattleCampus.Dashboard.Application.User.Query;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleCampus.Dashboard.API.Controllers
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

        [HttpPost("login")]
        public async Task<ActionResult<AdminUser>> LoginAsAdminAsync([FromBody]Login.Query query)
        {
            var adminUser = await _mediator.Send(query);

            return Ok(adminUser);
        }
    }
}