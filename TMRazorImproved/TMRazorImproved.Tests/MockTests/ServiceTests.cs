using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using Xunit;
using CommunityToolkit.Mvvm.Input;

namespace TMRazorImproved.Tests.MockTests
{
    public class ServiceTests
    {
        [Fact]
        public void SearchService_ShouldFindStaticItems()
        {
            // Arrange
            var search = new SearchService(NullLogger<SearchService>.Instance);
            var item = new SearchItem("Test Item", SearchCategory.Page, new RelayCommand(() => { }), "Description");
            search.RegisterItem(item);

            // Act
            var results = search.Search("test").ToList();

            // Assert
            Assert.Single(results);
            Assert.Equal("Test Item", results[0].Title);
        }

        [Fact]
        public void SearchService_ShouldFindDynamicItems()
        {
            // Arrange
            var search = new SearchService(NullLogger<SearchService>.Instance);
            search.RegisterCategory(SearchCategory.Script, () => new List<SearchItem> 
            {
                new SearchItem("DynamicScript", SearchCategory.Script, new RelayCommand(() => { }))
            });

            // Act
            var results = search.Search("dynamic").ToList();

            // Assert
            Assert.Single(results);
            Assert.Equal("DynamicScript", results[0].Title);
        }

        [Fact]
        public void SearchService_ShouldPrioritizeStartsWith()
        {
            // Arrange
            var search = new SearchService(NullLogger<SearchService>.Instance);
            search.RegisterItem(new SearchItem("Banana", SearchCategory.Page, new RelayCommand(() => { })));
            search.RegisterItem(new SearchItem("Apple", SearchCategory.Page, new RelayCommand(() => { })));
            search.RegisterItem(new SearchItem("Pineapple", SearchCategory.Page, new RelayCommand(() => { })));

            // Act
            var results = search.Search("apple").ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Equal("Apple", results[0].Title); // Should be first because it STARTS with "apple"
            Assert.Equal("Pineapple", results[1].Title);
        }
    }
}
