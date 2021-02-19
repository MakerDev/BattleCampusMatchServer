using BattleCampus.Core;
using BattleCampusMatchServer.Services.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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

            var timer = new System.Timers.Timer();
            timer.Interval = 30 * 1000; //15sec
            timer.Elapsed += CleanPendingLists;
        }

        private void CleanPendingLists(object sender, ElapsedEventArgs e)
        {
            var matchesToRemove = PendingMatches.Values.Where(p =>
            {
                var timespan = DateTime.Now - p.CreatedDate;

                return timespan.TotalSeconds >= 30;
            }).ToList();

            foreach (var matchToRemove in matchesToRemove)
            {
                PendingMatches.Remove(matchToRemove.MatchID, out var _);
                _logger.LogInformation($"Removed match : {matchToRemove} from pending list as it's been not used for the last 30seconds.");
            }

            var usersToRemove = PendingGameUsers.Keys.Where(p =>
            {
                var timespan = DateTime.Now - p.CreatedTime;

                return timespan.TotalSeconds >= 30;
            }).ToList();

            foreach (var userToRemove in usersToRemove)
            {
                PendingGameUsers.Remove(userToRemove, out var _);
                _logger.LogInformation($"Removed user : {userToRemove} from pending list as it's been not used for the last 30seconds.");
            }
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
        public bool ConnectUser(GameUser user)
        {
            var hasPendingUser = PendingGameUsers.TryGetValue(user, out var pendingMatchID);

            if (hasPendingUser == false)
            {
                _logger.LogError($"No such pending user {user}");
                return false;
            }

            var validMatch = Matches.TryGetValue(user.MatchID, out var match);

            if (validMatch == false)
            {
                //첫번째로 접속하려는 유저가 승격을 시킴. 
                validMatch = PendingMatches.TryGetValue(user.MatchID, out match);
                if (validMatch)
                {
                    _logger.LogWarning($"Got from {match} from pending match");
                    Matches.TryAdd(user.MatchID, match);
                }
                else
                {
                    _logger.LogError($"Failed to connect {user} as Match {user.MatchID} doesn't exist");
                    return false;
                }
            }
            else
            {
                _logger.LogWarning($"Got from {match} from normal match list");
            }

            match.Players.Add(user);

            _logger.LogInformation($"{user} is connected with connectionID : {user.ConnectionID} who is joining {match}");

            if (UserConnections.ContainsKey(user) == false)
            {
                UserConnections.AddOrUpdate(user, user.ConnectionID, (existingUser, existingConnectionId) =>
                {
                    existingUser.MatchID = match.MatchID;
                    //Update connectionId
                    existingUser.ConnectionID = user.ConnectionID;

                    return user.ConnectionID;
                });
            }
            else
            {
                _logger.LogWarning($"Duplicate connection with ID:{user.ConnectionID} and User:{user}");
            }

            //If all done successfully, then remove pending list.
            PendingMatches.TryRemove(pendingMatchID, out var _);
            PendingGameUsers.TryRemove(user, out var _);

            return true;
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

        private bool TryRemovePendingUserByID(Guid id, out GameUser removedUser)
        {
            var pendingUser = PendingGameUsers.Keys.FirstOrDefault(p => p.ID == id);

            if (pendingUser == null)
            {
                removedUser = null;
                return false;
            }

            var timespan = DateTime.Now - pendingUser.CreatedTime;

            if (timespan.TotalSeconds >= 10)
            {
                PendingGameUsers.TryRemove(pendingUser, out var _);

                removedUser = pendingUser;
                return true;
            }

            removedUser = null;
            return false;
        }
        private bool TryRemovePendingMatch(string matchID, out Match removedMatch)
        {
            var hasPendingMatch = PendingMatches.TryGetValue(matchID, out var pendingMatch);
            if (hasPendingMatch == false)
            {
                removedMatch = null;
                return false;
            }

            var timespan = DateTime.Now - pendingMatch.CreatedDate;
            if (timespan.TotalSeconds >= 10)
            {
                PendingMatches.TryRemove(matchID, out removedMatch);
                return true;
            }

            removedMatch = null;
            return false;
        }

        private bool TryAddToPendingMatches(Match match)
        {
            match.CreatedDate = DateTime.Now;

            var alreadyHasMatch = PendingMatches.TryGetValue(match.MatchID, out var pendingMatch);

            //Update if already exists.
            if (alreadyHasMatch)
            {
                pendingMatch.CreatedDate = DateTime.Now;
                return true;
            }

            var result = PendingMatches.TryAdd(match.MatchID, match);

            return result;
        }

        private bool TryAddToPendingUsers(GameUser user, string matchID)
        {
            user.CreatedTime = DateTime.Now;

            var pendingUser = PendingGameUsers.Keys.FirstOrDefault(p => p.ID == user.ID);

            //Update if already exists.
            if (pendingUser != null)
            {
                pendingUser.CreatedTime = DateTime.Now;
                return true;
            }

            var result = PendingGameUsers.TryAdd(user, matchID);

            return result;
        }

        private void RemovePlayerFromMatch(string matchID, GameUser user)
        {
            var hasMatch = Matches.TryGetValue(matchID, out var match);

            if (hasMatch == false)
            {
                _logger.LogError($"{user} tried to exit {matchID} which doesn't exist");
                return;
            }

            var player = match.Players.FirstOrDefault((x) => x.ID == user.ID);

            if (player == null)
            {
                _logger.LogError($"Failed to find user {user}");
                return;
            }

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

            TryAddToPendingUsers(user, matchID);

            Task.Delay(15000).ContinueWith(t =>
            {
                var removeSucess = TryRemovePendingUserByID(user.ID, out var removedUser);

                if (removeSucess)
                {
                    _logger.LogError($"Removed pending user {removedUser} as he failed to re-join to match {matchID}");
                }
            });

            if (match.CurrentPlayersCount <= 0)
            {
                //Move match to pending list
                Matches.Remove(matchID, out var _);
                var result = TryAddToPendingMatches(match);

                if (result)
                {
                    _logger.LogInformation($"Moved {match} to pending list as no more player is left");
                }
                else
                {
                    _logger.LogInformation($"Failed to move {match} to pending list.");
                }

                var t = Task.Delay(15000).ContinueWith((t) =>
                {
                    var hasRemovedMatch = TryRemovePendingMatch(matchID, out var deletedMatch);

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
                IpPortInfo = IpPortInfo,
                CreatedDate = DateTime.Now
            };


            //이런거 다 private internal method로 추출
            var userAddResult = TryAddToPendingUsers(host, matchID);
            if (userAddResult == false)
            {
                _logger.LogError($"Failed to add {host} to pending list");
                return new MatchCreationResult
                {
                    IsCreationSuccess = false,
                    CreationFailReason = $"Failed to add {host} to pending list",
                };
            }

            host.MatchID = match.MatchID;
            //HACK : 이제 호스트 시스템 없어..
            host.IsHost = false;

            var result = TryAddToPendingMatches(match);

            if (result == false)
            {
                _logger.LogError($"Failed to create {match.MatchID}");
                return new MatchCreationResult
                {
                    IsCreationSuccess = false,
                    CreationFailReason = $"{match.MatchID} is already taken",
                };
            }

            _logger.LogInformation($"{host.ID} successfully created match {match} on server {this}");

            //This is for the case where player quits game before he actually enters the GameScene so that BCNetworkManager,
            //can't detect player enter and exit.
            Task.Delay(10000).ContinueWith((t) =>
            {
                var pendingMatchRemoveResult = TryRemovePendingMatch(matchID, out var pendingMatch);

                if (pendingMatchRemoveResult)
                {
                    _logger.LogError($"Pending match {pendingMatch} has been removed as host failed to join the game.");
                }

                var userRemoveResult = TryRemovePendingUserByID(host.ID, out var pendingUser);

                if (userRemoveResult)
                {
                    _logger.LogError($"Pending User {pendingUser} has beed removed as he failed to join game {matchID} he started.");
                }
            });

            return new MatchCreationResult
            {
                IsCreationSuccess = true,
                Match = match,
            };
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

            var userAddResult = TryAddToPendingUsers(user, matchID);

            Task.Delay(10000).ContinueWith((t) =>
            {
                var userRemoveResult = TryRemovePendingUserByID(user.ID, out var pendingUser);

                if (userRemoveResult)
                {
                    _logger.LogError($"Pending User {pendingUser} has beed removed as he failed to join game {matchID}.");
                }
            });

            if (userAddResult)
            {
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
