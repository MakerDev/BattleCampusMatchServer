using BattleCampusMatchServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Server.Query
{
    public class GetGameServer
    {
        public class Query : IRequest<GameServer>
        {
            public Guid Id { get; set; }
        }

        public class Hander : IRequestHandler<Query, GameServer>
        {
            private readonly IMatchManager _matchManager;

            public Hander(IMatchManager matchManager)
            {
                _matchManager = matchManager;
            }

            public async Task<GameServer> Handle(Query request, CancellationToken cancellationToken)
            {
                //Logic goes here
                var servers = _matchManager.GetServers();

                return servers.FirstOrDefault(x => x.Id == request.Id);
            }
        }
    }
}
