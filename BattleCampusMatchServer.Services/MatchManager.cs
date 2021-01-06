using BattleCampusMatchServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleCampusMatchServer.Services
{
    public class MatchManager : IMatchManager
    {
        public Dictionary<string, Match> Matches { get; set; } = new Dictionary<string, Match>();

        public List<GameServer> Servers = new List<GameServer>();
        

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
            var server = Servers[0];
            //2. Try Create new match
            return server.CreateMatch(name);
        }

        public void RegisterGameServer(GameServer server)
        {
            Servers.Add(server);
        }

        public void UnRegisterGameServer(GameServer server)
        {
            Servers.Remove(server);
        }

        public List<Match> GetMatches()
        {
            return Matches.Values.ToList();
        }
    }
}
