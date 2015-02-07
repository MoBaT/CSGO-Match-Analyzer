using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    public class FlashGrenade : Grenade
    {
        public FlashGrenade()
        {
            PlayersFlashed = new List<RoundPlayer>();
        }
        public List<RoundPlayer> PlayersFlashed { get; set; }
    }
}
