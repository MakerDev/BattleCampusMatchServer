using BattleCampus.Core;
using BattleCampusMatchServer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleCampusMatchServer.Services
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
        private readonly object _matchLock = new object();
        /// <summary>
        /// Key : connectionId of the user
        /// Value : connected User
        /// </summary>
        public Dictionary<int, GameUser> UserConnections { get; private set; } = new Dictionary<int, GameUser>();
        private readonly object _userConnectionLock = new object();


        public GameServer(string name, IpPortInfo ipPortInfo, ILoggerFactory loggerFactory, int maxMatches = 5)
        {
            Name = name;
            IpPortInfo = ipPortInfo;
            MaxMatches = maxMatches;

            _logger = loggerFactory.CreateLogger<GameServer>();
            _logger.LogInformation($"{this} has been launched!");
        }

        public void DisconnectUser(int connectionID)
        {
            var hasUser = UserConnections.TryGetValue(connectionID, out var user);

            if (hasUser == false)
            {
                _logger.LogWarning($"Disconnecting with connectionID:{connectionID} failed");

                return;
            }

            if (user.MatchID != null)
            {
                RemovePlayerFromMatch(user.MatchID, user);
                _logger.LogInformation($"Disconnected {user} with connectionID: {connectionID} who was joining {user.MatchID}");
                user.MatchID = null;
            }

            lock (_userConnectionLock)
            {
                UserConnections.Remove(connectionID);
            }
        }

        public void ConnectUser(GameUser user)
        {
            var validMatch = Matches.TryGetValue(user.MatchID, out var match);

            if (validMatch == false)
            {
                _logger.LogError($"Failed to connect {user} as Match {user.MatchID} doesn't exist");
            }

            _logger.LogInformation($"{user} is connected with connectionID : {user.ConnectionID} who is joining {match}");

            if (UserConnections.ContainsKey(user.ConnectionID) == false)
            {
                lock (_userConnectionLock)
                {
                    UserConnections.Add(user.ConnectionID, user);
                }
            }
            else
            {
                _logger.LogWarning($"Duplicate connection with ID:{user.ConnectionID} and User:{user}");
            }
        }

        private void RemovePlayerFromMatch(string matchID, GameUser user)
        {
            var hasMatch = Matches.TryGetValue(matchID, out var match);

            if (hasMatch == false)
            {
                _logger.LogError($"{user} tried to exit {matchID} which doesn't exist");
                return;
            }

            bool removeResult;  

            lock (_matchLock)
            {
                removeResult = match.Players.Remove(user);
            }

            if (removeResult == false)
            {
                _logger.LogError($"{user} tried to exit {match} which he is not joining");
            }

            //Set current joining match to null;
            user.MatchID = null;

            //Delete Match itself if no more player is left.
            if (match.CurrentPlayersCount <= 0)
            {
                _logger.LogInformation($"Removed {match} as no more player is left");

                DeleteMatch(matchID);
            }
        }

        public MatchJoinResult JoinMatch(string matchID, GameUser user)
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

            user.MatchID = matchID;

            if (match.Players.Contains(user) == false)
            {
                lock (_matchLock)
                {
                    match.Players.Add(user);
                }

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

        public MatchCreationResult CreateMatch(string name, GameUser host)
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

            //한 서버 인스턴스당으로 매치 메이커가 작동하므로, 한 서버 안에서만 중복 안되면 괜찮기는 함.
            var matchID = Utils.GenerateMatchID();
            while (Matches.ContainsKey(matchID))
            {
                matchID = Utils.GenerateMatchID();
            }

            var match = new Match
            {
                MatchID = Utils.GenerateMatchID(),
                Name = name,
                IpPortInfo = IpPortInfo
            };

            host.MatchID = match.MatchID;

            lock (_matchLock)
            {
                match.Players.Add(host);
            }

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
            var hasMatch = Matches.ContainsKey(matchID);

            if (hasMatch == false)
            {
                _logger.LogError($"Tried to delete not existing match:{matchID} from {this}");
                return;
            }

            _logger.LogInformation($"Delete match : {matchID} from server {this}");

            lock (_matchLock)
            {
                Matches.Remove(matchID);
            }
        }

        public override string ToString()
        {
            return $"<{Name}={IpPortInfo}>";
        }
    }
}
