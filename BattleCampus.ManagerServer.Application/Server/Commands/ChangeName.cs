using Application.Errors;
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
    public class ChangeName
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
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
                var server = _context.Servers.Find(request.Id);

                if (server == null)
                {
                    throw new RestException(System.Net.HttpStatusCode.NotFound, "Server not found");
                }

                if (server.Name == request.Name)
                {
                    return Unit.Value;
                }

                server.Name = request.Name;

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
