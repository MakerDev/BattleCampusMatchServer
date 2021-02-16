using BattleCampus.Core;
using BattleCampusMatchServer.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Matches.Commands
{
    public class NotifyCompleteMatch
    {
        public class Command : IRequest
        {
            public IpPortInfo IpPortInfo { get; set; }
            public string MatchID { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly IMatchManager _matchManager;
            private readonly ILogger<NotifyCompleteMatch> _logger;

            public Handler(IMatchManager matchManager, ILogger<NotifyCompleteMatch> logger)
            {
                _matchManager = matchManager;
                _logger = logger;
            }

            public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var server = _matchManager.GetServer(request.IpPortInfo);

                if (server == null)
                {
                    _logger.LogError($"Failed to notify match completion as server {request.IpPortInfo} doesn't exist");

                    return Task.FromResult(Unit.Value);
                }

                server.NofityMatchComplete(request.MatchID);

                return Task.FromResult(Unit.Value);
            }
        }
    }
}
