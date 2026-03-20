using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMRazorImproved.Core.Services.Scripting.Api;

namespace TMRazorImproved.Core.Services.Scripting
{
    // ------------------------------------------------------------------
    // Data models
    // ------------------------------------------------------------------

    public record ParamDoc(string Name, string TypeName, bool HasDefault, string? DefaultValue);

    public record MethodDoc(
        string Name,
        string ReturnType,
        IReadOnlyList<ParamDoc> Parameters,
        string Description,
        bool IsStub)
    {
        /// <summary>Full signature string, e.g. "UseSkill(string name)".</summary>
        public string Signature =>
            $"{Name}({string.Join(", ", Parameters.Select(p => p.HasDefault
                ? $"{p.TypeName} {p.Name} = {p.DefaultValue}"
                : $"{p.TypeName} {p.Name}"))})";
    }

    public record ApiDoc(
        string VarName,
        string TypeName,
        string Description,
        IReadOnlyList<MethodDoc> Methods);

    // ------------------------------------------------------------------
    // Service
    // ------------------------------------------------------------------

    /// <summary>
    /// Genera documentazione Markdown e modelli strutturati per tutte le API
    /// di scripting tramite reflection + XML docs.  TASK-018.
    /// </summary>
    public class AutoDocService
    {
        /// <summary>
        /// Mappa: nome variabile Python → tipo C# dell'API.
        /// Deve rispecchiare le proprietà di <see cref="ScriptGlobals"/>.
        /// </summary>
        private static readonly (string Name, Type Type)[] ApiTypes =
        {
            ("Player",       typeof(PlayerApi)),
            ("Items",        typeof(ItemsApi)),
            ("Mobiles",      typeof(MobilesApi)),
            ("Misc",         typeof(MiscApi)),
            ("Journal",      typeof(JournalApi)),
            ("Gump",         typeof(GumpsApi)),
            ("Target",       typeof(TargetApi)),
            ("Skills",       typeof(SkillsApi)),
            ("Spells",       typeof(SpellsApi)),
            ("Statics",      typeof(StaticsApi)),
            ("Friend",       typeof(FriendApi)),
            ("Filters",      typeof(FiltersApi)),
            ("Timer",        typeof(TimerApi)),
            ("SpecialMoves", typeof(SpecialMovesApi)),
            ("Sound",        typeof(SoundApi)),
            ("Hotkey",       typeof(HotkeyApi)),
            ("AutoLoot",     typeof(AutoLootApi)),
            ("Dress",        typeof(DressApi)),
            ("Scavenger",    typeof(ScavengerApi)),
            ("Restock",      typeof(RestockApi)),
            ("Organizer",    typeof(OrganizerApi)),
            ("BandageHeal",  typeof(BandageHealApi)),
            ("PathFinding",  typeof(PathFindingApi)),
            ("Counter",      typeof(CounterApi)),
            ("DPSMeter",     typeof(DPSMeterApi)),
            ("PacketLogger", typeof(PacketLoggerApi)),
        };

        private readonly Dictionary<string, string> _xmlDocs;

        public AutoDocService()
        {
            _xmlDocs = LoadXmlDocs();
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        /// <summary>Returns structured data for all scripting APIs.</summary>
        public IReadOnlyList<ApiDoc> GetApis() =>
            ApiTypes.Select(t => BuildApiDoc(t.Name, t.Type)).ToList();

        /// <summary>Generates a full Markdown document.</summary>
        public string GenerateMarkdown()
        {
            var apis = GetApis();
            var sb = new StringBuilder();
            sb.AppendLine("# TMRazor Improved — Scripting API Reference");
            sb.AppendLine();
            sb.AppendLine($"> Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("## Available API Objects");
            sb.AppendLine();
            sb.AppendLine("| Object | Methods | Description |");
            sb.AppendLine("|--------|---------|-------------|");

            foreach (var api in apis)
                sb.AppendLine($"| `{api.VarName}` | {api.Methods.Count} | {api.Description} |");

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            foreach (var api in apis)
                GenerateApiSection(sb, api);

            return sb.ToString();
        }

        /// <summary>Writes the Markdown document to <paramref name="outputPath"/>.</summary>
        public async Task ExportAsync(string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(outputPath, GenerateMarkdown(), Encoding.UTF8);
        }

        // ------------------------------------------------------------------
        // Build model
        // ------------------------------------------------------------------

        private ApiDoc BuildApiDoc(string varName, Type type)
        {
            string classKey = $"T:{type.FullName}";
            string clasDesc = _xmlDocs.TryGetValue(classKey, out var cd)
                ? cd
                : GetFallbackTypeDescription(varName);

            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .OrderBy(m => m.Name)
                .Select(m => BuildMethodDoc(type, m))
                .ToList();

            return new ApiDoc(varName, type.Name, clasDesc, methods);
        }

        private MethodDoc BuildMethodDoc(Type type, MethodInfo method)
        {
            string xmlKey = BuildXmlKey(type, method);
            string description = _xmlDocs.TryGetValue(xmlKey, out var d) ? d : string.Empty;

            var parameters = method.GetParameters()
                .Select(p => new ParamDoc(
                    p.Name ?? "arg",
                    FormatTypeName(p.ParameterType),
                    p.HasDefaultValue,
                    p.HasDefaultValue ? FormatDefaultValue(p.DefaultValue) : null))
                .ToList();

            return new MethodDoc(
                method.Name,
                FormatTypeName(method.ReturnType),
                parameters,
                description,
                IsKnownStub(method.Name));
        }

        // ------------------------------------------------------------------
        // XML doc loading
        // ------------------------------------------------------------------

        private static Dictionary<string, string> LoadXmlDocs()
        {
            try
            {
                string xmlPath = Path.ChangeExtension(
                    typeof(AutoDocService).Assembly.Location, ".xml");

                if (!File.Exists(xmlPath)) return new();

                var doc = XDocument.Load(xmlPath);
                return doc.Descendants("member")
                    .Where(m => m.Attribute("name") != null)
                    .ToDictionary(
                        m => m.Attribute("name")!.Value,
                        m => CleanXmlText(m.Element("summary")?.Value ?? string.Empty),
                        StringComparer.Ordinal);
            }
            catch
            {
                return new();
            }
        }

        private static string CleanXmlText(string raw)
        {
            // Remove leading/trailing whitespace and normalize internal whitespace
            return string.Join(" ",
                raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(l => l.Trim())
                   .Where(l => l.Length > 0));
        }

        private static string BuildXmlKey(Type type, MethodInfo method)
        {
            // XML doc key format: M:FullTypeName.MethodName(ParamType1,ParamType2)
            var sb = new StringBuilder("M:");
            sb.Append(type.FullName);
            sb.Append('.');
            sb.Append(method.Name);

            var parms = method.GetParameters();
            if (parms.Length > 0)
            {
                sb.Append('(');
                sb.Append(string.Join(",", parms.Select(p => GetXmlTypeName(p.ParameterType))));
                sb.Append(')');
            }

            return sb.ToString();
        }

        private static string GetXmlTypeName(Type t)
        {
            if (t.IsGenericType)
            {
                string baseName = t.GetGenericTypeDefinition().FullName!;
                // Generic: `N → {N}
                baseName = baseName.Replace("`1", "{" + GetXmlTypeName(t.GetGenericArguments()[0]) + "}");
                return baseName;
            }
            return t.FullName ?? t.Name;
        }

        // ------------------------------------------------------------------
        // Markdown rendering
        // ------------------------------------------------------------------

        private static void GenerateApiSection(StringBuilder sb, ApiDoc api)
        {
            sb.AppendLine($"## `{api.VarName}`");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(api.Description))
            {
                sb.AppendLine(api.Description);
                sb.AppendLine();
            }

            if (api.Methods.Count == 0)
            {
                sb.AppendLine("*No public methods.*");
            }
            else
            {
                foreach (var m in api.Methods)
                {
                    sb.AppendLine($"### `{api.VarName}.{m.Signature}` → `{m.ReturnType}`");
                    sb.AppendLine();
                    if (!string.IsNullOrEmpty(m.Description))
                        sb.AppendLine(m.Description);
                    if (m.IsStub)
                        sb.AppendLine("> ⚠️ *Returns a placeholder value.*");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static string FormatTypeName(Type type)
        {
            if (type == typeof(void))   return "void";
            if (type == typeof(bool))   return "bool";
            if (type == typeof(int))    return "int";
            if (type == typeof(uint))   return "uint";
            if (type == typeof(long))   return "long";
            if (type == typeof(ulong))  return "ulong";
            if (type == typeof(short))  return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(byte))   return "byte";
            if (type == typeof(string)) return "string";
            if (type == typeof(double)) return "double";
            if (type == typeof(float))  return "float";
            if (type == typeof(object)) return "object";

            var nullable = Nullable.GetUnderlyingType(type);
            if (nullable != null) return $"{FormatTypeName(nullable)}?";

            if (type.IsGenericType)
            {
                string baseName = type.Name.Split('`')[0];
                string args = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
                return $"{baseName}<{args}>";
            }

            return type.Name;
        }

        private static string FormatDefaultValue(object? value)
        {
            if (value is null) return "null";
            if (value is string s) return $"\"{s}\"";
            if (value is bool b)   return b ? "true" : "false";
            return value.ToString() ?? "null";
        }

        private static readonly HashSet<string> _knownStubs = new(StringComparer.OrdinalIgnoreCase)
        {
            "GetMinDuration", "GetMaxDuration", "ChangeList", "CheckDeedHouse",
            "GetMapInfo", "ExportPythonAPI", "GetContPosition", "LastHotKey"
        };

        private static bool IsKnownStub(string name) => _knownStubs.Contains(name);

        private static string GetFallbackTypeDescription(string varName) => varName switch
        {
            "Player"       => "Player character stats, position, equipment, and status effects.",
            "Items"        => "Find, move, and inspect items in the world or containers.",
            "Mobiles"      => "Find and inspect NPCs and other players in the world.",
            "Misc"         => "Utility functions: pause, output, file I/O, menus, and queries.",
            "Journal"      => "Read and search the game journal (chat and system messages).",
            "Gump"         => "Interact with game windows (gumps): wait, read, and respond.",
            "Target"       => "Send targets to the server and wait for target cursors.",
            "Skills"       => "Read skill values and set skill locks.",
            "Spells"       => "Cast spells by name and check spell availability.",
            "Statics"      => "Query static map tiles and tile data.",
            "Friend"       => "Manage the friends list.",
            "Filters"      => "Toggle visual/audio filters (light, sound, mobile graphics).",
            "Timer"        => "Named timers for script timing logic.",
            "SpecialMoves" => "Activate weapon special abilities (primary and secondary).",
            "Sound"        => "Play custom sounds by ID.",
            "Hotkey"       => "Register and trigger hotkey actions from script.",
            "AutoLoot"     => "Control the AutoLoot agent (start, stop, change list).",
            "Dress"        => "Control the Dress agent (equip/unequip lists).",
            "Scavenger"    => "Control the Scavenger agent.",
            "Restock"      => "Control the Restock agent.",
            "Organizer"    => "Control the Organizer agent.",
            "BandageHeal"  => "Control the BandageHeal agent.",
            "PathFinding"  => "Compute paths and navigate to coordinates on the map.",
            "Counter"      => "Read item counters (counts items in backpack by graphic/hue).",
            "DPSMeter"     => "Read current DPS statistics from the DPS meter.",
            "PacketLogger" => "Start, stop, and control packet logging from script.",
            _              => string.Empty,
        };
    }
}
