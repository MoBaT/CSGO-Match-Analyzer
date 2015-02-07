using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchAnalyzer.Analyzer.Models
{
    public class RoundPlayerGrenade
    {
        public RoundPlayerGrenade()
        {
            Decoys = new List<Grenade>();
            Grenades = new List<Grenade>();
            Flashes = new List<FlashGrenade>();
            Smokes = new List<Grenade>();
            Fires = new List<Grenade>();
        }
        public List<Grenade> Decoys { get; set; }
        public List<Grenade> Grenades { get; set; }
        public List<FlashGrenade> Flashes { get; set; }
        public List<Grenade> Smokes { get; set; }
        public List<Grenade> Fires { get; set; }
    }
}
