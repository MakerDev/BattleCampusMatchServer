using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampus.Core
{
    public class GuestUser : GameUser
    {
        public GuestUser()
        {
            Name = "Guest";
            StudentID = "2020485485";
            ID = Guid.NewGuid();
        }
    }
}
