using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TMRazorImproved.Core.Utilities
{
    public static class LegacyMacroMigrator
    {
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
                {
                    newSteps.Add(migrated);
                }
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
                case "Pause":
                    return parts.Length >= 2 ? $"PAUSE {parts[1]}" : "PAUSE 1000";

                case "CastSpell":
                    // Format: CastSpell|id|name|target
                    return parts.Length >= 2 ? $"CAST {parts[1]}" : "";

                case "UseSkill":
                    // Format: UseSkill|name|target
                    // We need ID for USESKILL, but we can try to guess or just use 0
                    // Actually, many macros use names. We might need a lookup table.
                    return $"// TODO: USESKILL {parts[1]} (needs ID)";

                case "DoubleClick":
                    // Format: DoubleClick|mode|serial|...
                    if (parts.Length >= 3 && uint.TryParse(parts[2], out _))
                        return $"DOUBLECLICK {parts[2]}";
                    return $"// DOUBLECLICK mode {parts[1]}";

                case "Target":
                    // Format: Target|mode|serial|...
                    if (parts.Length >= 3 && uint.TryParse(parts[2], out _))
                        return $"TARGET {parts[2]}";
                    return "// TARGET advanced mode";

                case "WaitforTarget":
                case "WaitForTarget":
                    return parts.Length >= 2 ? $"WAITFORTARGET {parts[1]}" : "WAITFORTARGET 5000";

                case "AttackEntity":
                    if (parts.Length >= 3) return $"ATTACK {parts[2]}";
                    break;

                case "PickUp":
                    if (parts.Length >= 3) return $"// PICKUP {parts[1]} {parts[2]} (Not directly supported in simple macros)";
                    break;

                case "Drop":
                    return "// DROP (Not directly supported in simple macros)";

                case "Messaging":
                    // Messaging|type|msg|hue|target
                    if (parts.Length >= 3) return $"SAY {parts[2]}";
                    break;

                case "If":
                    return $"IF {parts[1]} {parts[2]} {parts[3]}"; // Simplified mapping

                case "ElseIf":
                    return $"ELSEIF {parts[1]} {parts[2]} {parts[3]}";

                case "else":
                    return "ELSE";

                case "EndIf":
                    return "ENDIF";

                case "While":
                    return $"WHILE {parts[1]} {parts[2]} {parts[3]}";

                case "EndWhile":
                    return "ENDWHILE";

                case "For":
                    return $"FOR {parts[1]}";

                case "EndFor":
                    return "ENDFOR";

                case "Comment":
                    return $"// {parts[1]}";

                case "Resync":
                    return "// RESYNC (Not supported in macros)";

                case "ClearJournal":
                    return "// CLEARJOURNAL";
            }

            return $"// UNMIGRATED: {line}";
        }
    }
}
