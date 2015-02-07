using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    public class RoundBombPlant : TimeTick
    {
        public RoundPlayer Player { get; set; }
        public bool Exploded { get; set; }
        public bool Defused { get; set; }
    }
}
