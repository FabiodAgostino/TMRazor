namespace TMRazorImproved.Shared.Models
{
    public class WeaponInfo
    {
        public string Name { get; set; } = string.Empty;
        public ushort Graphic { get; set; }
        public string Primary { get; set; } = string.Empty;
        public string Secondary { get; set; } = string.Empty;
        public bool TwoHanded { get; set; }
    }
}
