using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    class RoundTeam
    {
        public int PlayersKilled { get; set; }
        public int PlayersDead { get; set; }
        public int WeaponsFired { get; set; }
        public int DecoysThrown { get; set; }
        public int GrenadesThrown { get; set; }
        public int FlashesThrown { get; set; }
        public int SmokesThrown { get; set; }
        public int FiresThrown { get; set; }
        public bool WonRound { get; set; }
    }
}
