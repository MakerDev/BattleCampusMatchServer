using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampus.Core
{
    public class GameServerModel
    {
        public string Name { get; set; } = "Server";
        public int MaxMatches { get; set; } = 5;
        public IpPortInfo IpPortInfo { get; set; } = new IpPortInfo();
        /// <summary>
        /// Key : Match ID
        /// Value : Match instance
        /// </summary>
        public Dictionary<string, Match> Matches { get; set; } = new Dictionary<string, Match>();

        /// <summary>
        /// Key : connectionId of the user
        /// Value : connected User
        /// </summary>
        public Dictionary<int, GameUser> UserConnections { get; set; } = new Dictionary<int, GameUser>();
    }
}
