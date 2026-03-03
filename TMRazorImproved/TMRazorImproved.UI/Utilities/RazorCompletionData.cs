using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace TMRazorImproved.UI.Utilities
{
    /// <summary>
    /// Implements AvalonEdit ICompletionData to describe an entry in the completion list.
    /// </summary>
    public class RazorCompletionData : ICompletionData
    {
        public RazorCompletionData(string text, string description, bool isMethod = true)
        {
            Text = text;
            Description = description;
            IsMethod = isMethod;
        }

        public ImageSource? Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object Description { get; private set; }

        public double Priority => 0;

        public bool IsMethod { get; private set; }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // If it's a method, we might want to add parenthesis
            string insertionText = Text;
            if (IsMethod && !insertionText.EndsWith("()"))
            {
                // We could be smarter here and check if they are already there,
                // but for now let's keep it simple.
                // insertionText += "()";
            }
            
            textArea.Document.Replace(completionSegment, insertionText);
        }
    }
}
