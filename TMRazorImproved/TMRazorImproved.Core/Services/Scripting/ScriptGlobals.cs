using System.Threading;
using TMRazorImproved.Core.Services.Scripting.Api;

namespace TMRazorImproved.Core.Services.Scripting
{
    /// <summary>
    /// Classe globale iniettata negli script C# via Roslyn.
    /// Le proprietà pubbliche di questa classe saranno accessibili direttamente
    /// nello script senza prefisso (es. Player.Hits, ScriptToken.ThrowIfCancellationRequested()).
    /// </summary>
    public class ScriptGlobals
    {
        public PlayerApi Player { get; set; } = null!;
        public ItemsApi Items { get; set; } = null!;
        public MobilesApi Mobiles { get; set; } = null!;
        public MiscApi Misc { get; set; } = null!;
        public JournalApi Journal { get; set; } = null!;
        public GumpsApi Gump { get; set; } = null!;
        /// <summary>Alias per Gump — compatibilità RazorEnhanced.</summary>
        public GumpsApi Gumps { get => Gump; set => Gump = value; }
        public TargetApi Target { get; set; } = null!;
        public SkillsApi Skills { get; set; } = null!;
        public SpellsApi Spells { get; set; } = null!;
        public StaticsApi Statics { get; set; } = null!;
        public FriendApi Friend { get; set; } = null!;
        public FiltersApi Filters { get; set; } = null!;
        public TimerApi Timer { get; set; } = null!;
        public SpecialMovesApi SpecialMoves { get; set; } = null!;
        public SoundApi Sound { get; set; } = null!;
        public HotkeyApi Hotkey { get; set; } = null!;
        public AutoLootApi AutoLoot { get; set; } = null!;
        public DressApi Dress { get; set; } = null!;
        public ScavengerApi Scavenger { get; set; } = null!;
        public RestockApi Restock { get; set; } = null!;
        public OrganizerApi Organizer { get; set; } = null!;
        public BandageHealApi BandageHeal { get; set; } = null!;

        /// <summary>
        /// Token di cancellazione per lo script corrente.
        /// Gli script C# possono chiamare <c>ScriptToken.ThrowIfCancellationRequested()</c>
        /// nei propri loop per supportare la cancellazione cooperativa.
        /// </summary>
        public CancellationToken ScriptToken { get; set; }
    }
}
