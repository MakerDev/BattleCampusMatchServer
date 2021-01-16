using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampus.Core.Utils
{
    public enum EngineerClass
    {
        SIT = 193,

    }

    public class StudentClassExtractor
    {
        public static EngineerClass ExtractFromStudentID(string id)
        {
            var classId = id.Substring(3, 3);
            return (EngineerClass)Enum.Parse(typeof(EngineerClass), classId);
        }
    }
}
