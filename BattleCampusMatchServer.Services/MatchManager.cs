﻿using BattleCampusMatchServer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BattleCampusMatchServer.Services
{
    public class MatchManager : IMatchManager
    {
        //TODO : change this to handle multiple server instances on a single server
        /// <summary>
        /// Key : Server Ip
        /// </summary>
        public Dictionary<string, GameServer> Servers { get; private set; } = new Dictionary<string, GameServer>();

        private readonly ILogger<MatchManager> _logger;

        private int _lastUsedServerIndex = 0;

        public MatchManager(ILogger<MatchManager> logger)
        {
            _logger = logger;
        }

        public MatchCreationResult CreateNewMatch(string name, User host)
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

        public MatchJoinResult JoinMatch(string serverIp, string matchID, User user)
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

        public void NotifyPlayerExitGame(string serverIp, string matchID, User user)
        {
            var serverExists = Servers.TryGetValue(serverIp, out var server);

            if (serverExists == false)
            {
                //TODO : properly handle error here
                return;
            }

            server.RemovePlayerFromMatch(matchID, user);
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

        public void RegisterGameServer(GameServer server)
        {
            if (Servers.ContainsKey(server.IpPortInfo.IpAddress))
            {
                _logger.LogError($"Trying to register duplicate server => {server}");

                return;
            }

            Servers.Add(server.IpPortInfo.IpAddress, server);
            _logger.LogInformation($"Registerd server {server}");
        }

        public void UnRegisterGameServer(string ipAddress)
        {
            Servers.Remove(ipAddress);
            _logger.LogInformation($"Removed server => {ipAddress}");
        }
    }
}
