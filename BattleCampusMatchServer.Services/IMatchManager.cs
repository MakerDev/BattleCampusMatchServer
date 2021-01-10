using BattleCampusMatchServer.Models;
using System.Collections.Generic;

namespace BattleCampusMatchServer.Services
{
    public interface IMatchManager
    {
        void ConnectUser(string serverIp, User user);
        MatchCreationResult CreateNewMatch(string name, User host);
        void DisconnectUser(string serverIp, int connectionID);
        List<Match> GetMatches();
        MatchJoinResult JoinMatch(string serverIp, string matchID, User user);
        void NotifyPlayerExitGame(string serverIp, string matchID, User user);
        void RegisterGameServer(GameServer server);
        void UnRegisterGameServer(string ipAddress);
    }
}