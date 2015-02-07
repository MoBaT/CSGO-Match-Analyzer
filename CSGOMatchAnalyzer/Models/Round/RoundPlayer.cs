using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    public class RoundPlayer
    {
        public RoundPlayer()
        {
            PlayerRoundGrenade = new RoundPlayerGrenade();
            BombPlantAttempts = new List<RoundBombPlantAttempt>();
            BombDefuseAttempts = new List<RoundBombDefuseAttempt>();
        }

        public Player Player { get; set; }
        public RoundPlayerGrenade PlayerRoundGrenade { get; set; }
        public List<RoundBombPlantAttempt> BombPlantAttempts { get; set; }
        public List<RoundBombDefuseAttempt> BombDefuseAttempts { get; set; }
    }
}
