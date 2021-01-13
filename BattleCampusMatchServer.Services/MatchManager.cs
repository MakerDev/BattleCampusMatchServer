﻿using BattleCampus.Core;
using BattleCampus.Persistence;
using BattleCampusMatchServer.Models;
using BattleCampusMatchServer.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BattleCampusMatchServer.Services
{
    public class MatchManager : IMatchManager
    {
        //TODO : restore data from report server on start up

        //TODO : change this to handle multiple server instances on a single server
        /// <summary>
        /// Key : Server Ip
        /// </summary>
        public static Dictionary<IpPortInfo, GameServer> Servers { get; private set; } = null;
        private static int _lastUsedServerIndex = 0;

        private object _serverLock = new object();

        private readonly ILogger<MatchManager> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggerFactory _loggerFactory;

        public MatchManager(ILogger<MatchManager> logger, ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _loggerFactory = loggerFactory;

            //Load inital server info
            if (Servers == null)
            {
                var servers = dbContext.Servers
                .Include(x => x.IpPortInfo)
                .ToList();

                lock (_serverLock)
                {
                    Servers = new Dictionary<IpPortInfo, GameServer>();

                    foreach (var serverModel in servers)
                    {
                        var server = new GameServer(serverModel.Name, serverModel.IpPortInfo, _loggerFactory, serverModel.MaxMatches);

                        Servers.Add(server.IpPortInfo, server);
                    }
                }
            }
        }

        /// <summary>
        /// Relate user to a connection id, generated by user when entering the match
        /// </summary>
        /// <param name="user"></param>
        public void ConnectUser(IpPortInfo serverIpInfo, GameUser user)
        {
            var validServer = Servers.TryGetValue(serverIpInfo, out var server);

            if (validServer == false)
            {
                _logger.LogError($"Tried to connect {user} to {serverIpInfo} which does not exist");
                return;
            }

            server.ConnectUser(user);
        }

        /// <summary>
        /// Disconnect user from the server. As server doesn't hold user information as it has no UserManager,
        /// only connectionId can be passed.
        /// </summary>
        /// <param name="serverIp"></param>
        /// <param name="connectionID"></param>
        public void DisconnectUser(IpPortInfo serverIp, int connectionID)
        {
            var validServer = Servers.TryGetValue(serverIp, out var server);

            if (validServer == false)
            {
                _logger.LogError($"Tried to disconnect {connectionID} to {serverIp} which does not exist");
            }

            server.DisconnectUser(connectionID);
        }

        public MatchCreationResult CreateNewMatch(string name, GameUser host)
        {
            if (Servers.Count <= 0)
            {
                _logger.LogWarning($"No server available : User {host.Name} tried to create new match");

                return new MatchCreationResult
                {
                    IsCreationSuccess = false,
                    CreationFailReason = "No server available",
                };
            }

            //1. Find proper server : currently round robbin
            var server = Servers.Values.ToList()[_lastUsedServerIndex];
            _lastUsedServerIndex = (_lastUsedServerIndex + 1) % Servers.Count;
            _logger.LogInformation($"Server <{server.Name}|{server.IpPortInfo}> is selected for match <{name}>");

            //2. Try Create new match
            return server.CreateMatch(name, host);
        }

        public MatchJoinResult JoinMatch(IpPortInfo serverIp, string matchID, GameUser user)
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

            return server.JoinMatch(matchID, user);
        }


        //TODO : cache this
        //서버에서 매치정보관련 이벤트가 발생하면 내부 캐시를 업데이트 하는 식으로 가도 될 듯.
        public List<Match> GetMatches()
        {
            var matches = new List<Match>();

            foreach (var server in Servers.Values)
            {
                matches.AddRange(server.Matches.Values);
            }

            return matches;
        }

        public async Task RegisterGameServerAsync(string name, int maxMatches, IpPortInfo ipPortInfo)
        {
            var server = new GameServer(name, ipPortInfo, _loggerFactory);
            server.MaxMatches = maxMatches;

            if (Servers.ContainsKey(ipPortInfo))
            {
                _logger.LogError($"Trying to register duplicate server => {server}");

                return;
            }

            var alreadyHasServer = (await _dbContext.Servers.TryFindGameServerModel(ipPortInfo)) != null;

            if (alreadyHasServer)
            {
                _logger.LogError($"Already has {server} in the database");

                return;
            }

            Servers.Add(ipPortInfo, server);
            _dbContext.Servers.Add(new GameServerModel
            {
                IpPortInfo = ipPortInfo,
                MaxMatches = maxMatches,
                Name = name,
                State = ServerState.Running,
            });
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"Registerd server {server}");
        }

        public async Task TurnOffServerAsync(IpPortInfo ipPortInfo)
        {
            var serverModel = await _dbContext.Servers.TryFindGameServerModel(ipPortInfo);

            if (serverModel == null)
            {
                return;
            }

            Servers.Remove(ipPortInfo);
            serverModel.State = ServerState.Off;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnRegisterGameServerAsync(IpPortInfo ipPortInfo)
        {
            var serverModel = await _dbContext.Servers.TryFindGameServerModel(ipPortInfo);

            if (serverModel == null)
            {
                _logger.LogError($"{ipPortInfo} doesn't exist in the database");
            }
            else
            {
                _dbContext.Servers.Remove(serverModel);
                await _dbContext.SaveChangesAsync();
            }

            Servers.Remove(ipPortInfo);
            _logger.LogInformation($"Removed server => {ipPortInfo}");
        }
    }
}
