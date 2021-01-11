using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampus.Core
{
    /// <summary>
    /// Persisted user datas
    /// </summary>
    public class User
    {
        //Student id
        public string Id { get; set; } = null;
        public int TotalkillCount { get; set; } = 0;
        public int TotalPlayCount { get; set; } = 0;
    }
}
