using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampus.Dashboard.Domain
{
    public class GameServer
    {
        public string Name { get; set; } = "Server";
        public int MaxMatches { get; set; } = 5;
        public IpPortInfo IpPortInfo { get; set; } = new IpPortInfo();
    }
}
