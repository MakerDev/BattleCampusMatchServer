using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampus.Core
{
    public class GameServerModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "Server";
        public int MaxMatches { get; set; } = 5;
        public IpPortInfo IpPortInfo { get; set; } = new IpPortInfo();
        public ServerState State { get; set; }
    }
}
