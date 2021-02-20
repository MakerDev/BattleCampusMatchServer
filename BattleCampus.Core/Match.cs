using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BattleCampus.Core
{
    public class Match
    {
        public string Name { get; set; }
        public string MatchID { get; set; }
        public IpPortInfo IpPortInfo { get; set; }
        public int MaxPlayers { get; set; } = 6;
        public bool HasStarted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        //TODO: make this concurrent list?
        public ConcurrentDictionary<int, GameUser> Players { get; set; } = new ConcurrentDictionary<int, GameUser>();

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
                return (CurrentPlayersCount != MaxPlayers && HasStarted == false);
            }
        }

        public override string ToString()
        {
            return $"<{Name}-{MatchID}>";
        }
    }
}
