using BattleCampusMatchServer.Models;
using System.Collections.Generic;

namespace BattleCampusMatchServer.Services
{
    public interface IMatchManager
    {
        Dictionary<string, Match> Matches { get; set; }

        MatchCreationResult CreateNewMatch(string name);
        List<Match> GetMatches();
    }
}