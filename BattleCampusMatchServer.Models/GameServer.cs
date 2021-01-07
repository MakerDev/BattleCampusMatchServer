using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Models
{
    public class GameServer
    {
        public string Name { get; private set; } = "Server";
        public int MaxMatches { get; set; } = 5;
        public IpPortInfo IpPortInfo { get; private set; } = new IpPortInfo();
        /// <summary>
        /// Key : Match ID
        /// Value : Match instance
        /// This caches all matches of all server instances.
        /// </summary>
        public Dictionary<string, Match> AllMatches { get; private set; } = new Dictionary<string, Match>();

        public GameServer(string name, IpPortInfo ipPortInfo)
        {
            Name = name;
            IpPortInfo = ipPortInfo;
        }

        public MatchJoinResult JoinMatch(string matchID)
        {
            var result = AllMatches.TryGetValue(matchID, out var match);

            if (result == false)
            {
                return new MatchJoinResult
                {
                    JoinSucceeded = false,
                    JoinFailReason = $"Match: {matchID} doesn't exist.",
                };
            }

            if (match.CanJoin == false)
            {
                return new MatchJoinResult
                {
                    JoinFailReason = $"Match {matchID} is already full!",
                    JoinSucceeded = false,
                    Match = match
                };
            }

            //HACK : change this to actual user!
            match.Players.Add(new Player());

            return new MatchJoinResult
            {
                JoinSucceeded = true,
                JoinFailReason = "",
                Match = match,
            };
        }

        public MatchCreationResult CreateMatch(string name)
        {
            if (AllMatches.Count >= MaxMatches)
            {
                return new MatchCreationResult
                {
                    IsCreationSuccess = false,
                    CreationFailReason = "Server already full",
                    Match = null,
                };
            }

            var match = new Match
            {
                MatchID = Utils.GenerateMatchID(),
                Name = name,
                IpPortInfo = IpPortInfo
            };

            AllMatches.Add(match.MatchID, match);

            return new MatchCreationResult
            {
                IsCreationSuccess = true,
                Match = match,
            };
        }

        public void DeleteMatch(string matchID)
        {
            AllMatches.Remove(matchID);
        }
    }
}
