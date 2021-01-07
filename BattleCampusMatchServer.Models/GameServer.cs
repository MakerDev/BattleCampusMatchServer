using System;
using System.Collections.Generic;
using System.Linq;
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
        /// </summary>
        public Dictionary<string, Match> Matches { get; private set; } = new Dictionary<string, Match>();

        public GameServer(string name, IpPortInfo ipPortInfo)
        {
            Name = name;
            IpPortInfo = ipPortInfo;
        }

        public void RemovePlayerFromMatch(string matchID, User user)
        {
            var match = Matches[matchID];

            var player = match.Players.FirstOrDefault(x => x.ID == user.ID);

            if (player != null)
            {
                match.Players.Remove(player);
            }

            //Delete Match itself if no more player is left.
            if (match.CurrentPlayersCount <= 0)
            {
                Matches.Remove(matchID);
            }
        }

        public MatchJoinResult JoinMatch(string matchID, User user)
        {
            var result = Matches.TryGetValue(matchID, out var match);

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

            if (match.Players.Contains(user) == false)
            {
                match.Players.Add(user);
            }

            return new MatchJoinResult
            {
                JoinSucceeded = true,
                JoinFailReason = "",
                Match = match,
            };
        }

        public MatchCreationResult CreateMatch(string name, User host)
        {
            if (Matches.Count >= MaxMatches)
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

            match.Players.Add(host);

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
