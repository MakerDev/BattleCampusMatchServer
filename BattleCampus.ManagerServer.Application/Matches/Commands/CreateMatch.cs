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
    public class CreateMatch
    {
        public class Command : IRequest<MatchCreationResultDTO>
        {
            public string Name { get; set; }
            public GameUser User { get; set; }
        }

        public class Handler : IRequestHandler<Command, MatchCreationResultDTO>
        {
            private readonly IMatchManager _matchManager;

            public Handler(IMatchManager matchManager)
            {
                _matchManager = matchManager;
            }

            public Task<MatchCreationResultDTO> Handle(Command request, CancellationToken cancellationToken)
            {
                var matchCreationResult = _matchManager.CreateNewMatch(request.Name, request.User);

                if (matchCreationResult.IsCreationSuccess == false)
                {
                    return Task.FromResult(new MatchCreationResultDTO
                    {
                        IsCreationSuccess = false,
                        CreationFailReason = matchCreationResult.CreationFailReason,
                    });
                }

                var matchDTO = MatchDTO.CreateFromMatch(matchCreationResult.Match);

                return Task.FromResult(new MatchCreationResultDTO
                {
                    IsCreationSuccess = true,
                    Match = matchDTO,
                });
            }
        }
    }
}
