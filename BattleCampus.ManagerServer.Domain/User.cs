using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampus.ManagerServer.Domain
{
    /// <summary>
    /// This is persisted User. GameUser is temporary user for a single game connection
    /// </summary>
    public class User
    {
        public int StudentID { get; set; }
        
    }
}
