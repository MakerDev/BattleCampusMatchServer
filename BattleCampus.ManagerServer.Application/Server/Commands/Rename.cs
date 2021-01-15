using Application.Errors;
using BattleCampus.Persistence;
using BattleCampusMatchServer.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Server.Commands
{
    public class Rename
    {
        public class Command : IRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly ApplicationDbContext _context;
            private readonly IMatchManager _matchManager;

            public Handler(ApplicationDbContext context, IMatchManager matchManager)
            {
                _context = context;
                _matchManager = matchManager;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var server = await _context.Servers.FindAsync(request.Id);

                if (server == null)
                {
                    throw new RestException(HttpStatusCode.NotFound, $"Server {request.Id} is not found");
                }

                var hasSameName = (await _context.Servers.FirstOrDefaultAsync(x => x.Name == request.Name)) != null;

                if (hasSameName)
                {
                    throw new RestException(HttpStatusCode.BadRequest, $"Server with name {request.Name} already exists");
                }

                if (server.Name == request.Name)
                {
                    return Unit.Value;
                }

                server.Name = request.Name;
                _matchManager.RenameServer(server.IpPortInfo, request.Name);

                await _context.SaveChangesAsync();

                return Unit.Value;
            }
        }
    }
}
