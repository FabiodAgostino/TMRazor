using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IMacrosService
    {
        ObservableCollection<string> MacroList { get; }
        bool IsRecording { get; }
        bool IsPlaying { get; }
        string? ActiveMacro { get; }

        void LoadMacros();
        void Play(string name);
        void StopRecording();
        void StartRecording(string? name = null);
        void Save(string name, List<MacroStep> steps);
        void Delete(string name);
        void Rename(string oldName, string newName);
        List<MacroStep> GetSteps(string name);
        void SetAlias(string name, uint serial);
        void RemoveAlias(string name);
    }
}
