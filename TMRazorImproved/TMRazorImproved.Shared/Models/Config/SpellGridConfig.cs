using System.Collections.Generic;

namespace TMRazorImproved.Shared.Models.Config
{
    public class SpellGridConfig
    {
        public int Rows { get; set; } = 4;
        public int Columns { get; set; } = 8;
        public List<SpellIcon> Spells { get; set; } = new();
    }
}
