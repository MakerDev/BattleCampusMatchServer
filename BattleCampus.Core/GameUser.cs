using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampus.Core
{
    public class GameUser
    {
        public Guid ID { get; set; }
        public int ConnectionID { get; set; } = -1;
        public string StudentID { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; } = false;
        /// <summary>
        /// currently joining match id
        /// </summary>
        public string MatchID { get; set; } = null;


        public override string ToString()
        {
            return $"{Name}({StudentID})";
        }
    }
}
