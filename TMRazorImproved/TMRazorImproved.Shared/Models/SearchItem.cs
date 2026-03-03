using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace TMRazorImproved.Shared.Models
{
    public enum SearchCategory
    {
        Page,
        Script,
        Macro,
        Command,
        Agent
    }

    public class SearchItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "\uE11D"; // Default icon (Globe or similar)
        public SearchCategory Category { get; set; }
        public ICommand Command { get; set; } = new RelayCommand(() => { });
        public object? Parameter { get; set; }

        public SearchItem() { }

        public SearchItem(string title, SearchCategory category, ICommand command, string description = "", string icon = "")
        {
            Title = title;
            Category = category;
            Command = command;
            Description = description;
            if (!string.IsNullOrEmpty(icon)) Icon = icon;
        }
    }
}
