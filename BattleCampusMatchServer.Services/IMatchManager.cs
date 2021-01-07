using BattleCampusMatchServer.Models;
using System.Collections.Generic;

namespace BattleCampusMatchServer.Services
{
    public interface IMatchManager
    {
        MatchCreationResult CreateNewMatch(string name, User host);
        List<Match> GetMatches();
        MatchJoinResult JoinMatch(string serverIp, string matchID, User user);
        void NotifyPlayerExitGame(string serverIp, string matchID, User user);
        void RegisterGameServer(GameServer server);
        void UnRegisterGameServer(string ipAddress);
    }
}