using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Models.DTOs
{
    public class MatchDTO
    {
        public string Name { get; set; }
        public string MatchID { get; set; }
        public IpPortInfo IpPortInfo { get; set; }
        public int MaxPlayers { get; set; } = 6;
        public int CurrentPlayers { get; set; }
    }
}
