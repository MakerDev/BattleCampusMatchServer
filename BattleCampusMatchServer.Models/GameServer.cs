using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Models
{
    public class GameServer
    {
        public const int MAX_MATCHES = 5;

        public string Name { get; private set; } = "Server";
        public IpPortInfo IpPortInfo { get; private set; } = new IpPortInfo();
        /// <summary>
        /// Key : Match ID
        /// Value : Match instance
        /// </summary>
        public Dictionary<string, Match> Matches { get; set; } = new Dictionary<string, Match>();

        public GameServer(string name, IpPortInfo ipPortInfo)
        {
            Name = name;
            IpPortInfo = ipPortInfo;
        }

        public MatchCreationResult CreateMatch(string name)
        {
            if (Matches.Count >= MAX_MATCHES)
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

            Matches.Add(match.MatchID, match);

            return new MatchCreationResult
            {
                IsCreationSuccess = true,
                Match = match,
            };
        }

        public void DeleteMatch(string matchID)
        {
            Matches.Remove(matchID);
        }
    }
}
