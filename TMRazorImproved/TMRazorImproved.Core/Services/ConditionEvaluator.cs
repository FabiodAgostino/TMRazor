using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Valuta condizioni testuali rispetto allo stato corrente del mondo di gioco.
    /// Supporta le stesse 9 categorie del legacy IfAction.cs:
    ///   PlayerStats, PlayerStatus, Skill, Find, Count, InRange, TargetExists, InJournal, BuffExists.
    ///
    /// Sintassi condizioni:
    ///   [NOT] POISONED | HIDDEN | WARMODE | DEAD | PARALYZED | FLYING
    ///   [NOT] HP/MANA/STAM/STR/DEX/INT/WEIGHT/MAXHP/MAXMANA/MAXSTAM  &lt;op&gt; &lt;value&gt;
    ///   [NOT] SKILL &lt;skillName&gt; &lt;op&gt; &lt;value&gt;
    ///   [NOT] FIND &lt;graphic&gt; [&lt;range&gt;]
    ///   [NOT] COUNT &lt;graphic&gt; &lt;op&gt; &lt;value&gt;
    ///   [NOT] INRANGE &lt;serial&gt; &lt;range&gt;
    ///   [NOT] TARGETEXISTS
    ///   [NOT] INJOURNAL &lt;text&gt;
    ///   [NOT] BUFFEXISTS &lt;buffName&gt;
    /// </summary>
    public class ConditionEvaluator
    {
        private readonly IWorldService _world;
        private readonly ISkillsService? _skills;
        private readonly IJournalService? _journal;
        private readonly ITargetingService? _targeting;
        private readonly ILogger<ConditionEvaluator>? _logger;

        public ConditionEvaluator(
            IWorldService world,
            ISkillsService? skills = null,
            IJournalService? journal = null,
            ITargetingService? targeting = null,
            ILogger<ConditionEvaluator>? logger = null)
        {
            _world    = world;
            _skills   = skills;
            _journal  = journal;
            _targeting = targeting;
            _logger   = logger;
        }

        /// <summary>
        /// Valuta una stringa di condizione. Supporta prefisso opzionale "NOT ".
        /// </summary>
        public bool Evaluate(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;

            var cond   = condition.Trim();
            bool negate = false;
            if (cond.StartsWith("NOT ", StringComparison.OrdinalIgnoreCase))
            {
                negate = true;
                cond = cond.Substring(4).Trim();
            }

            bool result = EvaluateCore(cond);
            return negate ? !result : result;
        }

        private bool EvaluateCore(string cond)
        {
            var tokens = cond.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return true;

            string keyword = tokens[0].ToUpperInvariant();
            var player = _world.Player;

            switch (keyword)
            {
                // ── PlayerStatus ─────────────────────────────────────────────────
                case "POISONED":   return player?.IsPoisoned  ?? false;
                case "HIDDEN":     return player?.IsHidden    ?? false;
                case "WARMODE":    return player?.WarMode     ?? false;
                case "DEAD":       return player?.Hits == 0;
                case "PARALYZED": return player?.Paralyzed   ?? false;
                case "FLYING":    return player?.Flying      ?? false;
                case "YELLOWHITS": return player?.IsYellowHits ?? false;
                case "TARGETEXISTS":
                    // Controlla se c'è un cursore target attivo
                    return _targeting?.PendingCursorId != 0;

                // ── InJournal ────────────────────────────────────────────────────
                case "INJOURNAL":
                {
                    if (tokens.Length < 2) return false;
                    string searchText = string.Join(' ', tokens, 1, tokens.Length - 1);
                    return _journal?.Contains(searchText) ?? false;
                }

                // ── BuffExists ───────────────────────────────────────────────────
                case "BUFFEXISTS":
                {
                    if (tokens.Length < 2 || player == null) return false;
                    string buffName = string.Join(' ', tokens, 1, tokens.Length - 1);
                    return player.ActiveBuffs.ContainsKey(buffName);
                }

                // ── Skill ────────────────────────────────────────────────────────
                case "SKILL":
                {
                    // SKILL <skillName> <op> <value>
                    if (tokens.Length < 4 || _skills == null) return false;
                    string skillName = tokens[1];
                    string op        = tokens[2];
                    if (!double.TryParse(tokens[3], out double threshold)) return false;

                    var skill = _skills.Skills.FirstOrDefault(s =>
                        s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
                    if (skill == null) return false;

                    return CompareDouble(skill.Value, op, threshold);
                }

                // ── Find ─────────────────────────────────────────────────────────
                case "FIND":
                {
                    // FIND <graphic> [<range>]
                    if (tokens.Length < 2) return false;
                    if (!int.TryParse(tokens[1], System.Globalization.NumberStyles.Any, null, out int findGraphic))
                    {
                        // Prova con prefisso 0x
                        if (tokens[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                            findGraphic = Convert.ToInt32(tokens[1], 16);
                        else return false;
                    }
                    int findRange = tokens.Length >= 3 && int.TryParse(tokens[2], out int fr) ? fr : 18;

                    // Cerca item o mobile con quel graphic nel range
                    bool foundItem = _world.Items.Any(i =>
                        i.Graphic == findGraphic && IsInRange(i, player, findRange));
                    bool foundMobile = _world.Mobiles.Any(m =>
                        m.Graphic == findGraphic && IsInRange(m, player, findRange));
                    return foundItem || foundMobile;
                }

                // ── Count ────────────────────────────────────────────────────────
                case "COUNT":
                {
                    // COUNT <graphic> <op> <value>
                    if (tokens.Length < 4) return false;
                    if (!int.TryParse(tokens[1], System.Globalization.NumberStyles.Any, null, out int countGraphic))
                    {
                        if (tokens[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                            countGraphic = Convert.ToInt32(tokens[1], 16);
                        else return false;
                    }
                    string countOp = tokens[2];
                    if (!int.TryParse(tokens[3], out int countTarget)) return false;

                    // Conta gli item nel backpack con quel graphic
                    int count = 0;
                    var bp = player?.Backpack;
                    if (bp != null)
                        count = _world.GetItemsInContainer(bp.Serial)
                            .Where(i => i.Graphic == countGraphic)
                            .Sum(i => i.Amount > 0 ? i.Amount : 1);

                    return CompareInt(count, countOp, countTarget);
                }

                // ── InRange ──────────────────────────────────────────────────────
                case "INRANGE":
                {
                    // INRANGE <serial> <range>
                    if (tokens.Length < 3) return false;
                    uint inRangeSerial = tokens[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                        ? Convert.ToUInt32(tokens[1], 16)
                        : uint.TryParse(tokens[1], out uint irs) ? irs : 0;
                    if (!int.TryParse(tokens[2], out int inRangeRange)) return false;

                    var entity = _world.FindEntity(inRangeSerial);
                    return entity != null && IsInRange(entity, player, inRangeRange);
                }

                // ── PlayerStats (HP/MANA/STAM/STR/DEX/INT/WEIGHT...) ────────────
                default:
                {
                    if (tokens.Length < 3 || player == null) return false;
                    string op    = tokens[1];
                    if (!int.TryParse(tokens[2], out int val)) return false;

                    int pv = keyword switch
                    {
                        "HP"      or "HITS"       => player.Hits,
                        "MAXHP"   or "MAXHITS"    => (int)player.HitsMax,
                        "MANA"                    => player.Mana,
                        "MAXMANA"                 => (int)player.ManaMax,
                        "STAM"    or "STAMINA"    => player.Stam,
                        "MAXSTAM"                 => (int)player.StamMax,
                        "STR"                     => player.Str,
                        "DEX"                     => player.Dex,
                        "INT"                     => player.Int,
                        "WEIGHT"                  => player.Weight,
                        "MAXWEIGHT"               => player.MaxWeight,
                        "FOLLOWERS"               => player.Followers,
                        "MAXFOLLOWERS"            => player.FollowersMax,
                        "GOLD"                    => player.Gold,
                        "LUCK"                    => player.Luck,
                        "AR"      or "ARMOR"      => player.AR,
                        "FIRERESIST"              => player.FireResist,
                        "COLDRESIST"              => player.ColdResist,
                        "POISONRESIST"            => player.PoisonResist,
                        "ENERGYRESIST"            => player.EnergyResist,
                        _                         => int.MinValue
                    };

                    if (pv == int.MinValue)
                    {
                        _logger?.LogDebug("ConditionEvaluator: proprietà sconosciuta '{K}'", keyword);
                        return false;
                    }

                    return CompareInt(pv, op, val);
                }
            }
        }

        private static bool IsInRange(UOEntity entity, Mobile? player, int range)
        {
            if (player == null) return false;
            return entity.DistanceTo(player) <= range;
        }

        private static bool CompareInt(int actual, string op, int threshold) => op switch
        {
            "<"         => actual < threshold,
            ">"         => actual > threshold,
            "<="        => actual <= threshold,
            ">="        => actual >= threshold,
            "==" or "=" => actual == threshold,
            "!="        => actual != threshold,
            _           => false
        };

        private static bool CompareDouble(double actual, string op, double threshold) => op switch
        {
            "<"         => actual < threshold,
            ">"         => actual > threshold,
            "<="        => actual <= threshold,
            ">="        => actual >= threshold,
            "==" or "=" => Math.Abs(actual - threshold) < 0.001,
            "!="        => Math.Abs(actual - threshold) >= 0.001,
            _           => false
        };
    }
}
