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
    public class SyncConnections
    {
        public class Command : IRequest
        {
            public IpPortInfo IpPortInfo { get; set; }
            public List<int> ConnectionIDs { get; set; }
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
                var server = _matchManager.GetServer(request.IpPortInfo);

                if (server == null)
                {
                    return Task.FromResult(Unit.Value);
                }

                server.SyncCurrentConnections(request.ConnectionIDs);

                return Task.FromResult(Unit.Value);
            }
        }
    }
}
