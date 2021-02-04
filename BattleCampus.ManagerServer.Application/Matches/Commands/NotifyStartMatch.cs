using BattleCampus.Core;
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
    public class NotifyStartMatch
    {
        public class Command : IRequest
        {
            public IpPortInfo IpPortInfo { get; set; }
            public string MatchID { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly IMatchManager _matchManager;

            public Handler(IMatchManager matchManager)
            {
                _matchManager = matchManager;
            }

            public Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                _matchManager.NotifyMatchStarted(request.IpPortInfo, request.MatchID);

                return Task.FromResult(Unit.Value);
            }
        }
    }
}
