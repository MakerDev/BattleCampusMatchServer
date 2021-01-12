using BattleCampus.Core;
using BattleCampus.Persistence;
using BattleCampusMatchServer.Models.DTOs;
using BattleCampusMatchServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Matches.Query
{
    public class GetAll
    {
        public class Query : IRequest<List<Match>>
        {
            public Guid Id { get; set; }
        }

        public class Hander : IRequestHandler<Query, List<Match>>
        {
            private readonly IMatchManager _matchManager;

            public Hander(IMatchManager matchManager)
            {
                _matchManager = matchManager;
            }

            public Task<List<Match>> Handle(Query request, CancellationToken cancellationToken)
            {
                //Logic goes here
                return Task.FromResult(_matchManager.GetMatches());
            }
        }
    }
}
