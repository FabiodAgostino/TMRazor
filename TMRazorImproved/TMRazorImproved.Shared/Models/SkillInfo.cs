using CommunityToolkit.Mvvm.ComponentModel;

namespace TMRazorImproved.Shared.Models
{
    public enum SkillLock : byte
    {
        Up = 0,
        Down = 1,
        Lock = 2
    }

    public partial class SkillInfo : ObservableObject
    {
        public int ID { get; }
        public string Name { get; }

        [ObservableProperty]
        private double _baseValue;

        [ObservableProperty]
        private double _value;

        [ObservableProperty]
        private double _cap;

        [ObservableProperty]
        private SkillLock _lock;

        [ObservableProperty]
        private double _delta;

        public SkillInfo(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
