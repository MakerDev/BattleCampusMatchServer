using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleCampusMatchServer.Models
{
    public class GameServer
    {
        private readonly ILogger<GameServer> _logger;

        public string Name { get; private set; } = "Server";
        public int MaxMatches { get; set; } = 5;
        public IpPortInfo IpPortInfo { get; private set; } = new IpPortInfo();
        /// <summary>
        /// Key : Match ID
        /// Value : Match instance
        /// </summary>
        public Dictionary<string, Match> Matches { get; private set; } = new Dictionary<string, Match>();

        public GameServer(string name, IpPortInfo ipPortInfo, ILoggerFactory loggerFactory)
        {
            Name = name;
            IpPortInfo = ipPortInfo;
            _logger = loggerFactory.CreateLogger<GameServer>();
            _logger.LogInformation($"{this} has been launched!");
        }

        public void RemovePlayerFromMatch(string matchID, User user)
        {
            var hasMatch = Matches.TryGetValue(matchID, out var match);

            if (hasMatch == false)
            {
                _logger.LogError($"{user} tried to exit {matchID} which doesn't exist");
                return;
            }

            var player = match.Players.FirstOrDefault(x => x.ID == user.ID);

            if (player != null)
            {
                match.Players.Remove(player);
            }
            else
            {
                _logger.LogError($"{user} tried to exit {match} which he is not joining");
            }

            //Delete Match itself if no more player is left.
            if (match.CurrentPlayersCount <= 0)
            {
                _logger.LogInformation($"Removed {match} as no more player is left");

                DeleteMatch(matchID);
            }
        }

        public MatchJoinResult JoinMatch(string matchID, User user)
        {
            var result = Matches.TryGetValue(matchID, out var match);

            if (result == false)
            {
                _logger.LogError($"{user} tried to join {matchID} which doesn't exist");
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
                _logger.LogInformation($"{user} joined to {match}");
            }
            else
            {
                _logger.LogWarning($"{user} is already joining {match}");
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
                _logger.LogError($"Couldn't create match <{name}> as server {this} is already full");

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

            _logger.LogInformation($"Successfully created match {name} on server {this}");

            return new MatchCreationResult
            {
                IsCreationSuccess = true,
                Match = match,
            };
        }

        public void DeleteMatch(string matchID)
        {
            _logger.LogInformation($"Delete match : {matchID} from server {this}");
            Matches.Remove(matchID);
        }

        public override string ToString()
        {
            return $"<{Name}={IpPortInfo}>";
        }
    }
}
