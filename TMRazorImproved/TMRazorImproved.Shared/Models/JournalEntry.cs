using System;

namespace TMRazorImproved.Shared.Models
{
    public class JournalEntry
    {
        public string Text { get; }
        public string Name { get; }
        public uint Serial { get; }
        public ushort Hue { get; }
        public DateTime Timestamp { get; }

        public JournalEntry(string text, string name, uint serial, ushort hue)
        {
            Text = text;
            Name = name;
            Serial = serial;
            Hue = hue;
            Timestamp = DateTime.Now;
        }
    }
}
