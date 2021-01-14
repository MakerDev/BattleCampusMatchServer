using BattleCampus.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Services.Models.DTOs
{
    public class GameServerDTO
    {
        public string Name { get; set; }
        public IpPortInfo IpPortInfo { get; set; }
    }
}
