using System;
using System.Collections.Generic;

namespace BattleCampus.Core
{
    public class Match
    {
        public string Name { get; set; }
        public string MatchID { get; set; }
        public IpPortInfo IpPortInfo { get; set; }
        public int MaxPlayers { get; set; } = 6;
        //TODO: make this concurrent list?
        public List<GameUser> Players { get; set; } = new List<GameUser>();
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

        public override string ToString()
        {
            return $"<{Name}-{MatchID}>";
        }
    }
}
