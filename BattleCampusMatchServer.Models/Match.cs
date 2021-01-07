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
        public int CurrentPlayers
        {
            get
            {
                return Players.Count;
            }
        }
        public List<Player> Players { get; set; } = new List<Player>();
    }
}
