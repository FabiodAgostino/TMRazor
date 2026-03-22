using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels.Windows
{
    public partial class EditLootItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private LootItem _item;

        [ObservableProperty]
        private string _selectedPredefinedProperty = string.Empty;

        public ObservableCollection<PropertyFilter> PropertyFilters { get; } = new();

        public IRelayCommand AddPropertyCommand { get; }
        public IRelayCommand RemovePropertyCommand { get; }

        public static IReadOnlyList<string> PredefinedProperties { get; } = new List<string>
        {
            "Air Elemental Slayer",
            "Arachnid Slayer",
            "Balanced",
            "Blood Elemental Slayer",
            "Cold Damage",
            "Cold Resist",
            "Damage Increase",
            "Defense Chance Increase",
            "Demon Slayer",
            "Dexterity Bonus",
            "Dragon Slayer",
            "Earth Elemental Slayer",
            "Elemental Slayer",
            "Energy Damage",
            "Energy Resists",
            "Enhance Potion",
            "Faster Cast Recovery",
            "Faster Casting",
            "Fire Damage",
            "Fire Elemental Slayer",
            "Fire Resist",
            "Fireball Charges",
            "Gargoyle Slayer",
            "Gold Increase",
            "Greater Healing Charges",
            "Harm Charges",
            "Healing Charges",
            "Hit Chance Increase",
            "Hit Cold Area",
            "Hit Dispel",
            "Hit Energy Area",
            "Hit Fire Area",
            "Hit Fireball",
            "Hit Harm",
            "Hit Life Leech",
            "Hit Lightning",
            "Hit Lower Attack",
            "Hit Lower Defense",
            "Hit Magic Arrow",
            "Hit Mana Leech",
            "Hit Physical Area",
            "Hit Point Increase",
            "Hit Point Regeneration",
            "Hit Poison Area",
            "Hit Stamina Leech",
            "Intelligence Bonus",
            "Lightning Charges",
            "Lizardman Slayer",
            "Lower Mana Cost",
            "Lower Reagent Cost",
            "Lower Requirements",
            "Luck",
            "Mage Armor",
            "Magic Arrow Charges",
            "Mana Increase",
            "Mana Regeneration",
            "Night Sight",
            "Ogre Slayer",
            "Ophidian Slayer",
            "Orc Slayer",
            "Physical Damage",
            "Physical Resist",
            "Poison Damage",
            "Poison Elemental Slayer",
            "Poison Resist",
            "Reflect Physical Damage",
            "Repond Slayer",
            "Reptile Slayer",
            "Scorpion Slayer",
            "Self Repair",
            "Snake Slayer",
            "Snow Elemental Slayer",
            "Spell Channeling",
            "Spell Damage Increase",
            "Splintering Weapon",
            "Stamina Increase",
            "Stamina Regeneration",
            "Strength Bonus",
            "Swing Speed Increase",
            "Terathan Slayer",
            "Troll Slayer",
            "Undead Slayer",
            "Velocity",
            "Water Elemental Slayer",
        };

        public EditLootItemViewModel(LootItem item)
        {
            _item = item;
            foreach (var pf in item.PropertyFilters)
            {
                PropertyFilters.Add(new PropertyFilter(pf.Name, pf.MinValue, pf.MaxValue));
            }

            AddPropertyCommand = new RelayCommand(AddProperty);
            RemovePropertyCommand = new RelayCommand<PropertyFilter>(RemoveProperty);
        }

        private void AddProperty()
        {
            var name = string.IsNullOrWhiteSpace(SelectedPredefinedProperty) ? "New Property" : SelectedPredefinedProperty;
            PropertyFilters.Add(new PropertyFilter(name, 0, 100));
        }

        private void RemoveProperty(PropertyFilter? pf)
        {
            if (pf != null)
            {
                PropertyFilters.Remove(pf);
            }
        }

        public void Save()
        {
            Item.PropertyFilters.Clear();
            foreach (var pf in PropertyFilters)
            {
                Item.PropertyFilters.Add(pf);
            }
        }
    }
}
