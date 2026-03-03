using System.Collections.Generic;

namespace TMRazorImproved.Shared.Models
{
    public abstract class GumpControl
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; } = string.Empty;
        public virtual string DisplayName => Type;
    }

    public class GumpButton : GumpControl
    {
        public int ReleasedId { get; set; }
        public int PressedId { get; set; }
        public int Quit { get; set; }
        public int Page { get; set; }
        public int ButtonId { get; set; }
        public override string DisplayName => $"Button (ID: {ButtonId})";
    }

    public class GumpText : GumpControl
    {
        public int Color { get; set; }
        public int StringId { get; set; }
        public string Text { get; set; } = string.Empty;
        public override string DisplayName => $"Text: {Text}";
    }

    public class GumpLabel : GumpControl
    {
        public int ClilocId { get; set; }
        public string Args { get; set; } = string.Empty;
        public override string DisplayName => $"Label (Cliloc: {ClilocId})";
    }

    public class GumpImage : GumpControl
    {
        public int GumpId { get; set; }
        public override string DisplayName => $"Image (ID: {GumpId})";
    }

    public class GumpBackground : GumpControl
    {
        public int GumpId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public override string DisplayName => $"Background (ID: {GumpId})";
    }
}
