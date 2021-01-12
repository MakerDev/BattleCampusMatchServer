using BattleCampus.Core;
using BattleCampusMatchServer.Models;
using BattleCampusMatchServer.Models.DTOs;
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
    public class JoinMatch
    {
        public class Command : IRequest<MatchJoinResultDTO>
        {
            public string MatchID { get; set; }
            public IpPortInfo Server { get; set; }
            public GameUser User { get; set; }
        }

        public class Handler : IRequestHandler<Command, MatchJoinResultDTO>
        {
            private readonly IMatchManager _matchManager;

            public Handler(IMatchManager matchManager)
            {
                _matchManager = matchManager;
            }

            public Task<MatchJoinResultDTO> Handle(Command request, CancellationToken cancellationToken)
            {
                var joinResult = _matchManager.JoinMatch(request.Server, request.MatchID, request.User);

                MatchDTO match = null;

                if (joinResult.JoinSucceeded)
                {
                    match = MatchDTO.CreateFromMatch(joinResult.Match);
                }

                var matchJoinResult = new MatchJoinResultDTO
                {
                    JoinFailReason = joinResult.JoinFailReason,
                    JoinSucceeded = joinResult.JoinSucceeded,
                    Match = match,
                };

                return Task.FromResult(matchJoinResult);
            }
        }
    }
}
