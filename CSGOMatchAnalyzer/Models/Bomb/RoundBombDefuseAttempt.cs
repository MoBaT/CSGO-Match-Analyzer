using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    public class RoundBombDefuseAttempt : TimeTick
    {
        public RoundPlayer Player { get; set; }
        public bool HasKit { get; set; }
        public char Site { get; set; }
    }
}
