using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampus.Core
{
    public class GameUser
    {
        //TODO : This is currently client generated. Change this to be server-generated value.
        //As a GameUser can be a guest user, this ID doesn't have to be stored to database. 
        //It's generated on demand (ex:match joining, match creation) and disposed after a match is done.
        public Guid ID { get; set; }
        public int ConnectionID { get; set; } = -1;
        public string StudentID { get; set; }
        public string Name { get; set; }
        public bool IsHost { get; set; } = false;
        /// <summary>
        /// currently joining match id
        /// </summary>
        public string MatchID { get; set; } = null;

        public override bool Equals(object obj)
        {
            if (obj is not GameUser user)
            {
                return false;
            }

            return user.ID == ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return $"{ID}-{Name}({StudentID})";
        }
    }
}
