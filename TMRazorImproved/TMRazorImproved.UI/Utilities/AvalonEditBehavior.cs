using System;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace TMRazorImproved.UI.Utilities
{
    public static class AvalonEditBehavior
    {
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached("BindableText", typeof(string), typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableTextChanged));

        public static string GetBindableText(DependencyObject d)
        {
            return (string)d.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(DependencyObject d, string value)
        {
            d.SetValue(BindableTextProperty, value);
        }

        private static void OnBindableTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor)
            {
                if (editor.Document != null && e.NewValue is string newText)
                {
                    if (editor.Document.Text != newText)
                    {
                        editor.Document.Text = newText;
                    }
                }
            }
        }

        public static readonly DependencyProperty IsTextBindingActiveProperty =
            DependencyProperty.RegisterAttached("IsTextBindingActive", typeof(bool), typeof(AvalonEditBehavior),
                new PropertyMetadata(false, OnIsTextBindingActiveChanged));

        public static bool GetIsTextBindingActive(DependencyObject d)
        {
            return (bool)d.GetValue(IsTextBindingActiveProperty);
        }

        public static void SetIsTextBindingActive(DependencyObject d, bool value)
        {
            d.SetValue(IsTextBindingActiveProperty, value);
        }

        private static void OnIsTextBindingActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextEditor editor)
            {
                if ((bool)e.NewValue)
                {
                    editor.TextChanged += TextEditor_TextChanged;
                }
                else
                {
                    editor.TextChanged -= TextEditor_TextChanged;
                }
            }
        }

        private static void TextEditor_TextChanged(object? sender, EventArgs e)
        {
            if (sender is TextEditor editor)
            {
                SetBindableText(editor, editor.Document.Text);
            }
        }
    }
}
