using BattleCampusMatchServer.Models;
using System.Collections.Generic;

namespace BattleCampusMatchServer.Services
{
    public interface IMatchManager
    {
        MatchCreationResult CreateNewMatch(string name);
        List<Match> GetMatches();
        MatchJoinResult JoinMatch(string serverIp, string matchID);
        void RegisterGameServer(GameServer server);
        void UnRegisterGameServer(string ipAddress);
    }
}