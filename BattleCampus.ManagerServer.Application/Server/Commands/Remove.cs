using BattleCampus.Core;
using BattleCampus.ManageServer.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Server.Commands
{
    public class Remove
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly ApplicationDbContext _context;
            public Handler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                bool success = await _context.SaveChangesAsync() > 0;

                if (success == false)
                {
                    throw new Exception("Problem Saving Changes");
                }

                return Unit.Value;
            }
        }
    }
}
