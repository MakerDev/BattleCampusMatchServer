using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Models.DTOs
{
    public class GameServerDTO
    {
        public string Name { get; set; }
        public IpPortInfo IpPortInfo { get; set; }
    }
}
