using BattleCampusMatchServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BattleCampusMatchServer.Services
{
    public class MatchManager : IMatchManager
    {
        public Dictionary<string, GameServer> Servers = new Dictionary<string, GameServer>();

        public MatchManager()
        {
            //#if DEBUG
            //            var localHost = new IpPortInfo();
            //            var localServer = new GameServer("localDebug", localHost);
            //            Servers.Add(localHost.IpAddress, localServer);
            //#endif
        }

        public MatchCreationResult CreateNewMatch(string name)
        {
            if (Servers.Count <= 0)
            {
                return new MatchCreationResult
                {
                    IsCreationSuccess = false,
                    CreationFailReason = "No server available",
                };
            }

            //1. Find proper server
            var server = Servers.Values.ToList()[0];
            //2. Try Create new match
            return server.CreateMatch(name);
        }

        public void RegisterGameServer(GameServer server)
        {
            Servers.Add(server.IpPortInfo.IpAddress, server);
        }

        public void UnRegisterGameServer(string ipAddress)
        {
            Servers.Remove(ipAddress);
        }

        public MatchJoinResult JoinMatch(string serverIp, string matchID)
        {
            var serverExists = Servers.TryGetValue(serverIp, out var server);

            if (serverExists == false)
            {
                return new MatchJoinResult
                {
                    JoinFailReason = $"Server-{serverIp} does not exists",
                    JoinSucceeded = false,
                };
            }

            return server.JoinMatch(matchID);
        }

        //TODO : cache this
        //서버에서 매치정보관련 이벤트가 발생하면 내부 캐시를 업데이트 하는 식으로 가도 될 듯.
        public List<Match> GetMatches()
        {
            var matches = new List<Match>();

            foreach (var server in Servers.Values)
            {
                matches.AddRange(server.AllMatches.Values);
            }

            return matches;
        }
    }
}
