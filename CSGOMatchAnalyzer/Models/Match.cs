using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    class Match
    {
        public Match()
        {
            Players = new Dictionary<long, Player>();
            Rounds = new List<Round>();
        }

        public ulong MatchNumber { get; set; }
        public int TScore { get; set; }
        public int CTScore { get; set; }
        public string TClanName { get; set; }
        public string CTClanName { get; set; }
        public Dictionary<long, Player> Players { get; set; }
        public List<Round> Rounds { get; set; }
    }
}
