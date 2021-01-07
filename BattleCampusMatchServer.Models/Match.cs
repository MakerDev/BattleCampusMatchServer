using System;
using System.Collections.Generic;

namespace BattleCampusMatchServer.Models
{
    public class Match
    {
        public string Name { get; set; }
        public string MatchID { get; set; }
        public IpPortInfo IpPortInfo { get; set; }
        public int MaxPlayers { get; set; } = 6;
        public int CurrentPlayersCount
        {
            get
            {
                return Players.Count;
            }
        }

        public bool CanJoin
        {
            get
            {
                return CurrentPlayersCount != MaxPlayers;
            }
        }
        public List<User> Players { get; set; } = new List<User>();
    }
}
