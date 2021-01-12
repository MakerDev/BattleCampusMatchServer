﻿using BattleCampus.Core;
using BattleCampusMatchServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleCampusMatchServer.Services
{
    public interface IMatchManager
    {
        void ConnectUser(IpPortInfo serverIpInfo, GameUser user);
        MatchCreationResult CreateNewMatch(string name, GameUser host);
        void DisconnectUser(IpPortInfo serverIp, int connectionID);
        List<Match> GetMatches();
        MatchJoinResult JoinMatch(IpPortInfo serverIp, string matchID, GameUser user);
        Task RegisterGameServerAsync(string name, int maxMatches, IpPortInfo ipPortInfo);
        Task UnRegisterGameServerAsync(IpPortInfo serverIp);
    }
}