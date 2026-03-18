using System;
using System.Collections.Generic;

namespace TMRazorImproved.Core.Utilities
{
    /// <summary>
    /// Converte macro in formato legacy (RazorEnhanced) nel nuovo formato TMRazorImproved.
    /// Supporta tutti i 43 tipi di azione del sistema legacy.
    /// </summary>
    public static class LegacyMacroMigrator
    {
        // Tabella mapping nome skill → ID (per USESKILL)
        private static readonly Dictionary<string, int> SkillNameToId = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Alchemy",        0  }, { "Anatomy",         1  }, { "AnimalLore",      2  },
            { "ItemID",         3  }, { "ArmsLore",         4  }, { "Parrying",        5  },
            { "Begging",        6  }, { "Blacksmith",       7  }, { "Fletching",       8  },
            { "Peacemaking",    9  }, { "Camping",          10 }, { "Carpentry",       11 },
            { "Cartography",   12  }, { "Cooking",          13 }, { "DetectHidden",    14 },
            { "Discordance",   15  }, { "EvalInt",          16 }, { "Healing",         17 },
            { "Herding",       18  }, { "Hiding",           21 }, { "Provocation",     22 },
            { "Inscription",   23  }, { "Lockpicking",      24 }, { "Magery",          25 },
            { "MagicResist",   26  }, { "Tactics",          27 }, { "Snooping",        28 },
            { "Musicianship",  29  }, { "Poisoning",        30 }, { "Archery",         31 },
            { "SpiritSpeak",   32  }, { "Stealing",         33 }, { "Tailoring",       34 },
            { "AnimalTaming",  35  }, { "TasteID",          36 }, { "Tinkering",       37 },
            { "Tracking",      38  }, { "Veterinary",       39 }, { "Swords",          40 },
            { "Macing",        41  }, { "Fencing",          42 }, { "Wrestling",       43 },
            { "Lumberjacking",  44 }, { "Mining",           45 }, { "Meditation",      46 },
            { "Stealth",       47  }, { "RemoveTrap",       48 }, { "Necromancy",      49 },
            { "Focus",         50  }, { "Chivalry",         51 }, { "Bushido",         52 },
            { "Ninjitsu",      53  }, { "Spellweaving",     54 }, { "Mysticism",       59 },
            { "Imbuing",       56  }, { "Throwing",         57 }
        };

        public static List<string> Migrate(string legacyMacroContent)
        {
            var newSteps = new List<string>();
            var lines = legacyMacroContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            bool inActions = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("Actions:"))
                {
                    inActions = true;
                    continue;
                }

                if (!inActions) continue;

                var migrated = MigrateAction(line);
                if (!string.IsNullOrEmpty(migrated))
                    newSteps.Add(migrated);
            }

            return newSteps;
        }

        private static string MigrateAction(string line)
        {
            var parts = line.Split('|');
            if (parts.Length == 0) return "";

            string actionType = parts[0];

            switch (actionType)
            {
                // ── Controllo flusso ────────────────────────────────────────────────
                case "If":
                    // Format: If|property|operator|value
                    if (parts.Length >= 4)
                        return $"IF {parts[1]} {parts[2]} {parts[3]}";
                    return parts.Length >= 2 ? $"IF {parts[1]}" : "IF";

                case "ElseIf":
                    if (parts.Length >= 4)
                        return $"ELSEIF {parts[1]} {parts[2]} {parts[3]}";
                    return parts.Length >= 2 ? $"ELSEIF {parts[1]}" : "ELSEIF";

                case "Else":
                case "else":
                    return "ELSE";

                case "EndIf":
                    return "ENDIF";

                case "While":
                    if (parts.Length >= 4)
                        return $"WHILE {parts[1]} {parts[2]} {parts[3]}";
                    return parts.Length >= 2 ? $"WHILE {parts[1]}" : "WHILE";

                case "EndWhile":
                    return "ENDWHILE";

                case "For":
                    return parts.Length >= 2 ? $"FOR {parts[1]}" : "FOR 1";

                case "EndFor":
                    return "ENDFOR";

                case "Comment":
                    return parts.Length >= 2 ? $"// {parts[1]}" : "";

                // ── Tempo ───────────────────────────────────────────────────────────
                case "Pause":
                    return parts.Length >= 2 ? $"PAUSE {parts[1]}" : "PAUSE 1000";

                // ── Magia ───────────────────────────────────────────────────────────
                case "CastSpell":
                    // Format: CastSpell|id|name|target
                    return parts.Length >= 2 ? $"CAST {parts[1]}" : "";

                case "InvokeVirtue":
                    // Format: InvokeVirtue|virtueName
                    return parts.Length >= 2 ? $"INVOKEVIRTUE {parts[1]}" : "INVOKEVIRTUE honor";

                case "SetAbility":
                    // Format: SetAbility|primary|secondary (0=none,1=primary,2=secondary)
                    if (parts.Length >= 2)
                    {
                        return parts[1].Equals("secondary", StringComparison.OrdinalIgnoreCase)
                            ? "SETABILITY secondary"
                            : "SETABILITY primary";
                    }
                    return "SETABILITY primary";

                // ── Skill ───────────────────────────────────────────────────────────
                case "UseSkill":
                    // Format: UseSkill|name|target
                    if (parts.Length >= 2)
                    {
                        if (SkillNameToId.TryGetValue(parts[1], out int skillId))
                            return $"USESKILL {skillId}";
                        // Fallback: tenta parse diretto come numero
                        if (int.TryParse(parts[1], out int skillIdDirect))
                            return $"USESKILL {skillIdDirect}";
                        return $"// UseSkill: skill '{parts[1]}' non trovata nella tabella";
                    }
                    return "// UseSkill: argomenti insufficienti";

                // ── Click ───────────────────────────────────────────────────────────
                case "DoubleClick":
                    // Format: DoubleClick|mode|serial|...
                    if (parts.Length >= 3 && uint.TryParse(parts[2], out _))
                        return $"DOUBLECLICK {parts[2]}";
                    if (parts.Length >= 2)
                        return $"// DOUBLECLICK modalità: {parts[1]}";
                    return "// DOUBLECLICK";

                // ── Target ──────────────────────────────────────────────────────────
                case "Target":
                    // Format: Target|mode|serial|...
                    if (parts.Length >= 3 && uint.TryParse(parts[2], out _))
                        return $"TARGET {parts[2]}";
                    return "// TARGET modalità avanzata";

                case "WaitforTarget":
                case "WaitForTarget":
                    return parts.Length >= 2 ? $"WAITFORTARGET {parts[1]}" : "WAITFORTARGET 5000";

                case "TargetResource":
                    // Format: TargetResource|serial|resourceType
                    if (parts.Length >= 3)
                        return $"TARGETRESOURCE {parts[1]} {parts[2]}";
                    return "// TARGETRESOURCE: argomenti insufficienti";

                // ── Combattimento ───────────────────────────────────────────────────
                case "AttackEntity":
                    // Format: AttackEntity|mode|serial
                    if (parts.Length >= 3 && uint.TryParse(parts[2], out _))
                        return $"ATTACK {parts[2]}";
                    if (parts.Length >= 2 && uint.TryParse(parts[1], out _))
                        return $"ATTACK {parts[1]}";
                    return "// ATTACK";

                case "ArmDisarm":
                    // Format: ArmDisarm|hand (left/right/both)
                    return "ARMDISARM";

                // ── Messaggi ────────────────────────────────────────────────────────
                case "Messaging":
                    // Format: Messaging|type|msg|hue|target
                    if (parts.Length >= 3) return $"SAY {parts[2]}";
                    if (parts.Length >= 2) return $"SAY {parts[1]}";
                    return "// SAY";

                case "PromptResponse":
                    // Format: PromptResponse|text
                    return parts.Length >= 2 ? $"PROMPTRESPONSE {parts[1]}" : "PROMPTRESPONSE";

                case "QueryStringResponse":
                    // Format: QueryStringResponse|type|text (type: yes/no o testo libero)
                    if (parts.Length >= 3) return $"PROMPTRESPONSE {parts[2]}";
                    if (parts.Length >= 2) return $"PROMPTRESPONSE {parts[1]}";
                    return "PROMPTRESPONSE";

                // ── Gump ────────────────────────────────────────────────────────────
                case "WaitForGump":
                    // Format: WaitForGump|typeId|timeout
                    if (parts.Length >= 3)
                        return $"WAITFORGUMP {parts[1]} {parts[2]}";
                    if (parts.Length >= 2)
                        return $"WAITFORGUMP {parts[1]} 5000";
                    return "WAITFORGUMP 0 5000";

                case "GumpResponse":
                    // Format: GumpResponse|serial|typeId|buttonId
                    if (parts.Length >= 4)
                        return $"RESPONDGUMP {parts[1]} {parts[2]} {parts[3]}";
                    if (parts.Length >= 3)
                        return $"RESPONDGUMP {parts[1]} 0 {parts[2]}";
                    return "// RESPONDGUMP: argomenti insufficienti";

                // ── Item Management ──────────────────────────────────────────────────
                case "MoveItem":
                    // Format: MoveItem|mode|serial|dest|amount
                    if (parts.Length >= 5)
                        return $"MOVEITEM {parts[2]} {parts[3]} {parts[4]}";
                    if (parts.Length >= 4)
                        return $"MOVEITEM {parts[2]} {parts[3]} 1";
                    if (parts.Length >= 3)
                        return $"MOVEITEM {parts[2]} 0 1";
                    return "// MOVEITEM: argomenti insufficienti";

                case "PickUp":
                    // Format: PickUp|mode|serial|amount
                    if (parts.Length >= 4)
                        return $"PICKUP {parts[2]} {parts[3]}";
                    if (parts.Length >= 3)
                        return $"PICKUP {parts[2]} 1";
                    return "// PICKUP: argomenti insufficienti";

                case "Drop":
                    // Format: Drop|mode|serial|x|y|z   oppure   Drop|serial
                    if (parts.Length >= 6)
                        return $"DROP {parts[2]} {parts[3]} {parts[4]} {parts[5]}";
                    if (parts.Length >= 2)
                        return $"DROP {parts[1]} 0 0 0";
                    return "// DROP: argomenti insufficienti";

                case "Bandage":
                    // Format: Bandage|mode|serial
                    if (parts.Length >= 3 && uint.TryParse(parts[2], out uint bandTarget) && bandTarget != 0)
                        return $"BANDAGE {parts[2]}";
                    return "BANDAGE";

                case "UsePotion":
                    // Format: UsePotion|potionType
                    if (parts.Length >= 2)
                    {
                        string potType = parts[1].ToLowerInvariant() switch
                        {
                            "heal" or "healpotion"    => "heal",
                            "cure" or "curepotion"    => "cure",
                            "refresh" or "refreshpotion" => "refresh",
                            "agility" or "agilitypotion" => "agility",
                            "strength" or "strengthpotion" => "strength",
                            "explosion" or "explosionpotion" => "explosion",
                            "poison" or "poisonpotion" => "poison",
                            "nightsight" => "nightsight",
                            _ => parts[1].ToLower()
                        };
                        return $"USEPOTIONTYPE {potType}";
                    }
                    return "USEPOTIONTYPE heal";

                case "UseContextMenu":
                    // Format: UseContextMenu|mode|serial|entry
                    if (parts.Length >= 4)
                        return $"USECONTEXTMENU {parts[2]} {parts[3]}";
                    if (parts.Length >= 3)
                        return $"USECONTEXTMENU {parts[2]} 0";
                    return "// USECONTEXTMENU: argomenti insufficienti";

                // ── Mount ───────────────────────────────────────────────────────────
                case "Mount":
                    // Format: Mount|mode|serial
                    if (parts.Length >= 3 && uint.TryParse(parts[2], out _))
                        return $"MOUNT {parts[2]}";
                    return "MOUNT";

                // ── Alias ───────────────────────────────────────────────────────────
                case "SetAlias":
                    // Format: SetAlias|name|serial
                    if (parts.Length >= 3)
                        return $"SETALIAS {parts[1]} {parts[2]}";
                    return "// SETALIAS: argomenti insufficienti";

                case "RemoveAlias":
                    // Format: RemoveAlias|name
                    return parts.Length >= 2 ? $"REMOVEALIAS {parts[1]}" : "// REMOVEALIAS";

                // ── Movimento ───────────────────────────────────────────────────────
                case "Movement":
                    // Format: Movement|direction|run (il movimento diretto non è supportato in macro)
                    if (parts.Length >= 3)
                        return $"// MOVE direction={parts[1]} run={parts[2]} (movimento non supportato)";
                    return "// MOVE (movimento non supportato)";

                case "Fly":
                    return "FLY";

                // ── Stato giocatore ─────────────────────────────────────────────────
                case "ToggleWarMode":
                    return "WARMODE toggle";

                // ── Agenti ──────────────────────────────────────────────────────────
                case "RunOrganizerOnce":
                    // Format: RunOrganizerOnce|agentName
                    return "RUNORGANIZER";

                // ── Utility ─────────────────────────────────────────────────────────
                case "ClearJournal":
                    return "CLEARJOURNAL";

                case "Resync":
                    return "RESYNC";

                case "Disconnect":
                    return "// DISCONNECT (non supportato in macro)";

                case "RenameMobile":
                    // Format: RenameMobile|serial|name
                    if (parts.Length >= 3)
                        return $"RENAMEMOBILE {parts[1]} {parts[2]}";
                    return "// RENAMEMOBILE: argomenti insufficienti";

                default:
                    return $"// UNMIGRATED: {line}";
            }
        }
    }
}
