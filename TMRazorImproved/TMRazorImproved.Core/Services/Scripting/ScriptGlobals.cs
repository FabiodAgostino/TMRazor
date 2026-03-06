using TMRazorImproved.Core.Services.Scripting.Api;

namespace TMRazorImproved.Core.Services.Scripting
{
    /// <summary>
    /// Classe globale iniettata negli script C#.
    /// Le proprietà pubbliche di questa classe saranno accessibili direttamente
    /// nello script senza prefisso (es. Player.Hits).
    /// </summary>
    public class ScriptGlobals
    {
        public PlayerApi Player { get; set; } = null!;
        public ItemsApi Items { get; set; } = null!;
        public MobilesApi Mobiles { get; set; } = null!;
        public MiscApi Misc { get; set; } = null!;
        public JournalApi Journal { get; set; } = null!;
        public GumpsApi Gumps { get; set; } = null!;
        public TargetApi Target { get; set; } = null!;
        public SkillsApi Skills { get; set; } = null!;
        public SpellsApi Spells { get; set; } = null!;
        public StaticsApi Statics { get; set; } = null!;
        public FriendApi Friend { get; set; } = null!;
        public FiltersApi Filters { get; set; } = null!;
        public TimerApi Timer { get; set; } = null!;
    }
}
