using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Models.DTOs
{
    public class MatchJoinResultDTO
    {
        public MatchDTO Match { get; set; } = null;
        public bool JoinSucceeded { get; set; } = false;
        public string JoinFailReason { get; set; } = "No such match exists";
    }
}
