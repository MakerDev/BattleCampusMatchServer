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

        public bool TryMakeHost(GameUser user)
        {
            var connectedUser = UserConnections.Keys.FirstOrDefault(x => x.ID == user.ID);

            if (connectedUser == null)
            {
                _logger.LogError($"Failed to make {user} host of Match {user.MatchID} as the user is not connected.");
                return false;
            }

            var isPendingMatch = PendingMatches.TryGetValue(user.MatchID, out var match);
            var isMatch = Matches.TryGetValue(user.MatchID, out match);

            if (isPendingMatch == false && isMatch == false)
            {
                _logger.LogError($"Failed to make {user} host of Match {user.MatchID} as the match doesn't exist");
                return false;
            }

            if (isPendingMatch)
            {
                //Move to Matches
                PendingMatches.TryRemove(user.MatchID, out match);
                Matches.TryAdd(user.MatchID, match);
            }

            return false;
        }

        /// <summary>
        /// User must come with proper matchID
        /// </summary>
        /// <param name="user"></param>
        public void ConnectUser(GameUser user)
        {
            Match match;

            var hasPendingUser = PendingGameUsers.TryRemove(user, out var pendingMatchID);

            if (hasPendingUser == false)
            {
                _logger.LogError($"No such pending user {user}");
                return;
            }

            var validMatch = Matches.TryGetValue(user.MatchID, out match);

            if (validMatch == false)
            {
                validMatch = PendingMatches.TryRemove(user.MatchID, out match);

                if (validMatch)
                {
                    Matches.TryAdd(user.MatchID, match);
                }
                else
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

            var player = match.Players.Find((x) => x.ID == user.ID);
            var removeResult = match.Players.Remove(player);

            if (removeResult == false)
            {
                _logger.LogError($"{user} tried to exit {match} which he is not joining");
            }

            //Move player to pending list.
            //여기서 User connection의 matchID를 초기화 해 주어야 나중에 disconnect시 오류가 발생하지 않음
            var connectedUser = UserConnections.Keys.FirstOrDefault(x => x.ID == user.ID);
            if (connectedUser != null)
            {
                connectedUser.MatchID = null;
            }

            PendingGameUsers.TryAdd(user, matchID);

            Task.Delay(8000).ContinueWith(t =>
            {
                var userRemoved = PendingGameUsers.TryRemove(user, out var _);

                if (userRemoved)
                {
                    _logger.LogError($"Removed pending user {user} as he failed to re-join to match {matchID}");
                }
            });

            if (match.CurrentPlayersCount <= 0)
            {
                _logger.LogInformation($"Moved {match} to pending list as no more player is left");

                //Move match to pending list
                Matches.Remove(matchID, out var _);
                PendingMatches.TryAdd(matchID, match);

                //Remove pending match after 30sec.
                //이게 기가 막힌 타이밍에 겹치면 새로운 PendingList에 추가하자마자, Remove가 실행되면서, 새롭게 PendingList에 추가된 애를
                //바로 지워버리고, 이러면 애들이 join을 못하게 된다. 따라서, 단순 매치 ID만 가지고 지우면 안되고, 이 연산에 대한 Guid를 함께 봐야한다.
                var t = Task.Delay(30000).ContinueWith((t) =>
                {
                    var hasRemovedMatch = PendingMatches.Remove(matchID, out var deletedMatch);

                    if (hasRemovedMatch)
                    {
                        _logger.LogError($"Pending match {deletedMatch} has been removed as no player is joining this match.");
                    }
                });                
            }
        }

        public void NofityMatchComplete(string matchID)
        {
            var hasMatch = Matches.TryGetValue(matchID, out var match);

            if (hasMatch == false)
            {
                _logger.LogError($"Cannot notify completion {matchID}, as it doesn't exist");
                return;
            }

            match.HasStarted = false;

            //Foreach를 쓰면, 리스트 자체가 변경되어서 exception발생
            //Remove all players
            for (int i = 0; i < match.Players.Count; i++)
            {
                RemovePlayerFromMatch(matchID, match.Players[0]);
            }

            _logger.LogInformation($"Match {match} is completed!");
        }

        public void NotifyMatchStarted(string matchID)
        {
            var hasMatch = Matches.TryGetValue(matchID, out var match);

            if (hasMatch == false)
            {
                _logger.LogError($"Cannot start {matchID}, as it doesn't exist");
                return;
            }

            match.HasStarted = true;
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
                if (match.HasStarted)
                {
                    return new MatchJoinResult
                    {
                        JoinFailReason = $"Match {matchID} has already started.",
                        JoinSucceeded = false,
                        Match = match
                    };
                }
                else
                {
                    return new MatchJoinResult
                    {
                        JoinFailReason = $"Match {matchID} is already full!",
                        JoinSucceeded = false,
                        Match = match
                    };
                }
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

            //HACK : 이제 호스트 시스템 없어..
            host.IsHost = false;

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
            Task.Delay(8000).ContinueWith((t) =>
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

        /// <summary>
        /// This is only for admins to kill problematic matches.
        /// </summary>
        /// <param name="matchID"></param>
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
