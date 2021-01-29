using BattleCampus.Core;
using BattleCampusMatchServer.Services.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampusMatchServer.Services
{
    public class GameServer
    {
        private readonly ILogger<GameServer> _logger;

        public Guid Id { get; private set; }
        public string Name { get; set; } = "Server";
        public int MaxMatches { get; set; } = 5;
        public IpPortInfo IpPortInfo { get; private set; } = new IpPortInfo();
        /// <summary>
        /// Key : Match ID
        /// Value : Match instance
        /// </summary>
        public ConcurrentDictionary<string, Match> Matches { get; private set; }

        /// <summary>
        /// Matches pending to be displayed for match list. When creating match, created match doesn't directly be added
        /// to Matches above, for the case host user exits game before it enters GameScene, and NetworkManager doesn't
        /// handle user's state correctly.
        /// </summary>
        public ConcurrentDictionary<string, Match> PendingMatches { get; private set; }

        /// <summary>
        /// Key : Pending User
        /// Value : The match that the user is wating to join.
        /// When creating or joining a match, the GameUser is added to this dictionary. 
        /// When user is connected, then it's added to the match's Players list. 
        /// </summary>
        public ConcurrentDictionary<GameUser, string> PendingGameUsers { get; private set; } = new ConcurrentDictionary<GameUser, string>();

        /// <summary>
        /// Key : Connected User. Users are compared equality by ID
        /// Value : User's ConnectionID in game.
        /// </summary>
        public ConcurrentDictionary<GameUser, int> UserConnections { get; private set; }

        public GameServer(GameServerModel gameServerModel, ILoggerFactory loggerFactory, int maxMatches = 5)
        {
            Id = gameServerModel.Id;
            Name = gameServerModel.Name;
            IpPortInfo = gameServerModel.IpPortInfo;
            MaxMatches = maxMatches;

            Matches = new ConcurrentDictionary<string, Match>(Environment.ProcessorCount * 2, 128);
            UserConnections = new ConcurrentDictionary<GameUser, int>(Environment.ProcessorCount * 2, 256);
            PendingMatches = new ConcurrentDictionary<string, Match>(Environment.ProcessorCount * 2, 64);

            _logger = loggerFactory.CreateLogger<GameServer>();
            _logger.LogInformation($"GameServer : {this} has been launched!");
        }

        public void ResetServer()
        {
            UserConnections.Clear();
            Matches.Clear();
        }

        public void ConnectUser(GameUser user)
        {
            Match match;

            var hasPendingUser = PendingGameUsers.TryRemove(user, out var pendingMatchID);

            if (hasPendingUser == false)
            {
                _logger.LogError($"No such pending user {user}");
                return;
            }

            if (user.IsHost)
            {
                var validMatch = PendingMatches.TryGetValue(user.MatchID, out match);

                if (validMatch == false)
                {
                    _logger.LogError($"Failed to connect host user <{user}> as Match {user.MatchID} doesn't exist");
                    return;
                }

                PendingMatches.Remove(user.MatchID, out var _);
                Matches.TryAdd(user.MatchID, match);

                //match.Players[0] = user;
            }
            else
            {
                var validMatch = Matches.TryGetValue(user.MatchID, out match);

                if (validMatch == false)
                {
                    _logger.LogError($"Failed to connect {user} as Match {user.MatchID} doesn't exist");
                    return;
                }
            }

            match.Players.Add(user);

            _logger.LogInformation($"{user} is connected with connectionID : {user.ConnectionID} who is joining {match}");

            if (UserConnections.ContainsKey(user) == false)
            {
                UserConnections.AddOrUpdate(user, user.ConnectionID, (existingUser, existingConnectionId) =>
                {
                    //Update connectionId
                    existingUser.ConnectionID = user.ConnectionID;

                    return user.ConnectionID;
                });
            }
            else
            {
                _logger.LogWarning($"Duplicate connection with ID:{user.ConnectionID} and User:{user}");
            }
        }

        //This is called by server. As server only knows connection Id for the client, GameUser cannot be passed.
        public void DisconnectUser(int connectionID)
        {
            var user = UserConnections.Keys.FirstOrDefault(x => x.ConnectionID == connectionID);

            if (user == null)
            {
                _logger.LogWarning($"Disconnecting with connectionID:{connectionID} failed. No such connection exists");

                return;
            }

            if (user.MatchID != null)
            {
                _logger.LogInformation($"Disconnected {user} with connectionID: {connectionID} who was joining {user.MatchID}");
                RemovePlayerFromMatch(user.MatchID, user);
                user.MatchID = null;
            }

            UserConnections.TryRemove(user, out var _);
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

            var player = match.Players.Find((x) => x.ID == user.ID);
            removeResult = match.Players.Remove(player);

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

            var userAddResult = PendingGameUsers.TryAdd(user, matchID);

            if (userAddResult)
            {
                //match.Players.Add(user);

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

        private string GenerateUniqueMatchID()
        {
            var matchID = Utils.GenerateMatchID();
            while (Matches.ContainsKey(matchID))
            {
                matchID = Utils.GenerateMatchID();
            }

            return matchID;
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
            var matchID = GenerateUniqueMatchID();

            var match = new Match
            {
                MatchID = matchID,
                Name = name,
                IpPortInfo = IpPortInfo
            };

            //이런거 다 private internal method로 추출
            var userAddResult = PendingGameUsers.TryAdd(host, matchID);
            if (userAddResult == false)
            {
                _logger.LogError($"Failed to add {host} to pending list");
                return new MatchCreationResult
                {
                    IsCreationSuccess = false,
                    CreationFailReason = $"Failed to add {host} to pending list",
                };
            }
            //match.Players.Add(host);

            host.MatchID = match.MatchID;

            var result = PendingMatches.TryAdd(match.MatchID, match);

            if (result == false)
            {
                _logger.LogError($"Failed to create {match.MatchID}");
                return new MatchCreationResult
                {
                    IsCreationSuccess = false,
                    CreationFailReason = $"{match.MatchID} is already taken",
                };
            }

            _logger.LogInformation($"{host.ID} successfully created match {name} on server {this}");

            //This is for the case where player quits game before he actually enters the GameScene so that BCNetworkManager,
            //can't detect player enter and exit.
            Task.Delay(5000).ContinueWith((t) =>
            {
                var hasRemovedUser = PendingGameUsers.Remove(host, out var _);
                var hasRemoved = PendingMatches.Remove(matchID, out var deletedMatch);

                if (hasRemovedUser)
                {
                    _logger.LogError($"Host user {host} has been removed as host failed to join the game.");
                }

                if (hasRemoved)
                {
                    _logger.LogError($"Match {deletedMatch} has been removed as host failed to join the game.");
                }
            });

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

            Matches.Remove(matchID, out var _);
        }

        public override string ToString()
        {
            return $"<{Name}={IpPortInfo}>";
        }
    }
}
