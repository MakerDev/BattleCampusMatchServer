using BattleCampus.Core;
using BattleCampusMatchServer.Models;
using System.Collections.Generic;

namespace BattleCampusMatchServer.Services
{
    public interface IMatchManager
    {
        void ConnectUser(string serverIp, GameUser user);
        MatchCreationResult CreateNewMatch(string name, GameUser host);
        void DisconnectUser(string serverIp, int connectionID);
        List<Match> GetMatches();
        MatchJoinResult JoinMatch(string serverIp, string matchID, GameUser user);
        void NotifyPlayerExitGame(string serverIp, string matchID, GameUser user);
        void RegisterGameServer(GameServer server);
        void UnRegisterGameServer(string ipAddress);
    }
}