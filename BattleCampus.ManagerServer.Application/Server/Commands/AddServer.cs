using Application.Errors;
using BattleCampus.Core;
using BattleCampus.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Server.Commands
{
    public class AddServer
    {
        public class Command : IRequest
        {
            public string Name { get; set; }
            public int MaxMatches { get; set; }
            public IpPortInfo IpPortInfo { get; set; }
            public ServerState State { get; set; }
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
                var server = await _context.Servers.FirstOrDefaultAsync((x) => x.IpPortInfo.Equals(request.IpPortInfo));

                if (server != null)
                {
                    throw new RestException(System.Net.HttpStatusCode.BadRequest, $"Server {request.IpPortInfo} already exists");
                }

                server = new GameServerModel
                {
                    IpPortInfo = request.IpPortInfo,
                    MaxMatches = request.MaxMatches,
                    Name = request.Name,
                    State = request.State,
                };

                _context.Servers.Add(server);

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
