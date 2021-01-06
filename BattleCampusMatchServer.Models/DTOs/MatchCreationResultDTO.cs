using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Models.DTOs
{
    public class MatchCreationResultDTO
    {
        public bool IsCreationSuccess { get; set; } = false;
        public MatchDTO Match { get; set; }
        public string CreationFailReason { get; set; } = "";
    }
}
