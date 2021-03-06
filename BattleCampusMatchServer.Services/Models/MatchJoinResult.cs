﻿using BattleCampus.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Services.Models
{
    public class MatchJoinResult
    {
        public Match Match { get; set; } = null;
        public bool JoinSucceeded { get; set; } = false;
        public string JoinFailReason { get; set; } = "No such match exists";
    }
}
