using BattleCampus.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampusMatchServer.Services.Extensions
{
    public static class GameServerDbSetExtensions
    {
        public static async Task<GameServerModel> TryFindGameServerModel(this DbSet<GameServerModel> servers, IpPortInfo ipPortInfo)
        {
            var server = await servers.FirstOrDefaultAsync(x =>
                x.IpPortInfo.IpAddress == ipPortInfo.IpAddress
                && x.IpPortInfo.DesktopPort == ipPortInfo.DesktopPort
                && x.IpPortInfo.WebsocketPort == ipPortInfo.WebsocketPort);

            return server;
        }
    }
}
