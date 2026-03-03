using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ISearchService
    {
        void RegisterItem(SearchItem item);
        void UnregisterItem(SearchItem item);
        void RegisterCategory(SearchCategory category, Func<IEnumerable<SearchItem>> provider);
        IEnumerable<SearchItem> Search(string query);
        void Clear();
    }
}
