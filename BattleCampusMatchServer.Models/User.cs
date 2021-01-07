using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Models
{
    public class User
    {
        public Guid ID { get; set; }
        public string StudentID { get; set; }
        public string Name { get; set; }
    }
}
