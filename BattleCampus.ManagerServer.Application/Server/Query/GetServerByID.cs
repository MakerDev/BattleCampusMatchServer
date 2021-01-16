using Application.Errors;
using BattleCampus.Core;
using BattleCampus.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.MatchServer.Application.Server.Query
{
    public class GetServerByID
    {
        public class Query : IRequest<GameServerModel>
        {
            public Guid Id { get; set; }
        }

        public class Hander : IRequestHandler<Query, GameServerModel>
        {
            private readonly ApplicationDbContext _context;

            public Hander(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<GameServerModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var server = await _context.Servers.FindAsync(request.Id);

                if (server == null)
                {
                    throw new RestException(HttpStatusCode.NotFound, $"server with id {request.Id} is not found");
                }

                return server;
            }
        }
    }
}
