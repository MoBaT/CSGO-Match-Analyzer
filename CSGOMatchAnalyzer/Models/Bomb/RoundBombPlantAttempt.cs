using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    enum RoundBombType
    {
        StartPlant = 0,
        AbortPlant = 1,
        StartDefuse = 2,
        AbsortDefuse = 3
    }

    public class RoundBombPlantAttempt : TimeTick
    {
        public RoundPlayer Player { get; set; }
        public char Site { get; set; }

        public RoundBombPlant BombPlant = new RoundBombPlant();
    }
}
