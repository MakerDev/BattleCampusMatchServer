using BattleCampusMatchServer.Models;
using System.Collections.Generic;

namespace BattleCampusMatchServer.Services
{
    public interface IMatchManager
    {
        MatchCreationResult CreateNewMatch(string name);
        List<Match> GetMatches();
        void RegisterGameServer(GameServer server);
        void UnRegisterGameServer(string ipAddress);
    }
}