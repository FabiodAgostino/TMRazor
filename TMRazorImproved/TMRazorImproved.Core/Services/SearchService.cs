using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services
{
    public class SearchService : ISearchService
    {
        private readonly List<SearchItem> _staticItems = new();
        private readonly Dictionary<SearchCategory, Func<IEnumerable<SearchItem>>> _dynamicProviders = new();
        private readonly ILogger<SearchService> _logger;

        public SearchService(ILogger<SearchService> logger)
        {
            _logger = logger;
        }

        public void RegisterItem(SearchItem item)
        {
            if (item == null) return;
            lock (_staticItems)
            {
                if (!_staticItems.Any(i => i.Title == item.Title && i.Category == item.Category))
                {
                    _staticItems.Add(item);
                    _logger.LogTrace("SearchItem registered: {Title} ({Category})", item.Title, item.Category);
                }
            }
        }

        public void UnregisterItem(SearchItem item)
        {
            if (item == null) return;
            lock (_staticItems)
            {
                _staticItems.RemoveAll(i => i.Title == item.Title && i.Category == item.Category);
            }
        }

        public void RegisterCategory(SearchCategory category, Func<IEnumerable<SearchItem>> provider)
        {
            lock (_dynamicProviders)
            {
                _dynamicProviders[category] = provider;
            }
        }

        public IEnumerable<SearchItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Return a subset of items or empty if no query
                return GetFullList().Take(10);
            }

            string q = query.ToLowerInvariant();
            var allItems = GetFullList();

            return allItems
                .Where(i => i.Title.ToLowerInvariant().Contains(q) || i.Description.ToLowerInvariant().Contains(q))
                .OrderBy(i => i.Title.ToLowerInvariant().StartsWith(q) ? 0 : 1) // Prioritize exact starts
                .ThenBy(i => i.Title.Length) // Then shorter titles
                .Take(20);
        }

        public void Clear()
        {
            lock (_staticItems) _staticItems.Clear();
            lock (_dynamicProviders) _dynamicProviders.Clear();
        }

        private List<SearchItem> GetFullList()
        {
            var list = new List<SearchItem>();
            lock (_staticItems)
            {
                list.AddRange(_staticItems);
            }

            // Snapshot the providers to execute them outside the lock (P2-04)
            List<Func<IEnumerable<SearchItem>>> providers;
            lock (_dynamicProviders)
            {
                providers = _dynamicProviders.Values.ToList();
            }

            foreach (var provider in providers)
            {
                try
                {
                    var items = provider();
                    if (items != null)
                        list.AddRange(items);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing search dynamic provider");
                }
            }
            return list;
        }
    }
}
