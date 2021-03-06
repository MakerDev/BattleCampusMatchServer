﻿using BattleCampus.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleCampusMatchServer.Services.Models
{
    public class MatchCreationResult
    {
        public bool IsCreationSuccess { get; set; } = false;
        public Match Match { get; set; } = null;
        public string CreationFailReason { get; set; } = "";
    }
}
