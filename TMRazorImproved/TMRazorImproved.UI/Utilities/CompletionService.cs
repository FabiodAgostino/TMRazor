using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace TMRazorImproved.UI.Utilities
{
    public class CompletionService
    {
        private static readonly Dictionary<string, List<ICompletionData>> _cache = new();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                // We reflect on the Core assembly to find all API classes
                // Assembly name is TMRazorImproved.Core
                var coreAssembly = Assembly.Load("TMRazorImproved.Core");
                var apiNamespace = "TMRazorImproved.Core.Services.Scripting.Api";

                var apiTypes = coreAssembly.GetTypes()
                    .Where(t => t.IsClass && t.IsPublic && t.Namespace == apiNamespace && t.Name.EndsWith("Api"));

                foreach (var type in apiTypes)
                {
                    var prefix = type.Name.Replace("Api", "");
                    var items = new List<ICompletionData>();

                    // Get all public instance methods (virtual or not)
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(m => !m.IsSpecialName); // Exclude getters/setters

                    foreach (var method in methods)
                    {
                        var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                        items.Add(new RazorCompletionData(method.Name, $"{method.Name}({parameters})"));
                    }

                    // Get all public instance properties
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var prop in properties)
                    {
                        items.Add(new RazorCompletionData(prop.Name, $"{prop.Name} ({prop.PropertyType.Name})", isMethod: false));
                    }

                    _cache[prefix] = items.OrderBy(i => i.Text).ToList();
                }

                // Global variables like "Player", "Items", "Mobiles", etc.
                var globals = new List<ICompletionData>();
                foreach (var key in _cache.Keys)
                {
                    globals.Add(new RazorCompletionData(key, $"RazorEnhanced API: {key}", isMethod: false));
                }
                _cache["globals"] = globals.OrderBy(i => i.Text).ToList();

                _initialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CompletionService Init Error: {ex.Message}");
            }
        }

        public static List<ICompletionData> GetCompletionData(string? context)
        {
            if (!_initialized) Initialize();

            if (string.IsNullOrEmpty(context))
            {
                return _cache.TryGetValue("globals", out var items) ? items : new List<ICompletionData>();
            }

            return _cache.TryGetValue(context, out var contextItems) ? contextItems : new List<ICompletionData>();
        }
    }
}
