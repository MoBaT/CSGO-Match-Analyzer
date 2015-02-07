using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    class Round
    {
        public Round()
        {
            Team1 = new RoundTeam();
            Team2 = new RoundTeam();
            Players = new Dictionary<long, RoundPlayer>();
        }

        public int RoundNumber { get; set; }
        public bool BombDefused { get; set; }
        public RoundTeam Team1 { get; set; }
        public RoundTeam Team2 { get; set; }
        public Dictionary<long, RoundPlayer> Players { get; set; }


        public List<RoundBombPlantAttempt> BombPlantAttempts { get { return Players.SelectMany(i => i.Value.BombPlantAttempts).ToList(); } }
        public List<RoundBombDefuseAttempt> BombDefuseAttempts { get { return Players.SelectMany(i => i.Value.BombDefuseAttempts).ToList(); } }
    }
}
