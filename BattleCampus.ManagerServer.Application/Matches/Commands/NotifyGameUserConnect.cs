﻿using BattleCampus.Core;
using BattleCampusMatchServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Matches.Commands
{
    public class NotifyGameUserConnect
    {
        public class Command : IRequest<bool>
        {
            public IpPortInfo IpPortInfo { get; set; }
            public GameUser User { get; set; }
        }

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly IMatchManager _matchManager;

            public Handler(IMatchManager matchManager)
            {
                _matchManager = matchManager;
            }

            public Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = _matchManager.ConnectUser(request.IpPortInfo, request.User);

                return Task.FromResult(result);
            }
        }
    }
}
