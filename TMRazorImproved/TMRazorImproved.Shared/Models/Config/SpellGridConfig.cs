using System.Collections.Generic;

namespace TMRazorImproved.Shared.Models.Config
{
    public class SpellGridConfig
    {
        public double X { get; set; } = double.NaN;
        public double Y { get; set; } = double.NaN;
        public int Rows { get; set; } = 4;
        public int Columns { get; set; } = 8;
        public List<SpellIcon> Spells { get; set; } = new();
    }
}
