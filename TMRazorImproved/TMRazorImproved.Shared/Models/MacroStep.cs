namespace TMRazorImproved.Shared.Models
{
    public class MacroStep
    {
        public string Command { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;

        public MacroStep() { }

        public MacroStep(string command, string description)
        {
            Command = command;
            Description = description;
        }

        public override string ToString() => Command;
    }
}
