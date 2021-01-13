using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampus.Core
{
    public class GuestUser : GameUser
    {
        public GuestUser()
        {
            ID = Guid.NewGuid();
            Name = "Guest";
            StudentID = "2020485485";
        }
    }
}
