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

namespace BattleCampus.MatchServer.Application.Server.Query
{
    public class GetAllFromDb
    {
        public class Query : IRequest<List<GameServerModel>>
        {
        }

        public class Hander : IRequestHandler<Query, List<GameServerModel>>
        {
            private readonly ApplicationDbContext _context;

            public Hander(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<List<GameServerModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                //Logic goes here
                return await _context.Servers.ToListAsync();
            }
        }
    }
}
