using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using IeltsTeachingAssistant.Helpers;

namespace IeltsTeachingAssistant.Controls
{
    public static class RichDescriptorTextBlock
    {
        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(string),
                typeof(RichDescriptorTextBlock),
                new PropertyMetadata(string.Empty, OnFormattedTextChanged));

        public static string GetFormattedText(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.Inlines.Clear();
                string newText = e.NewValue as string ?? string.Empty;

                var inlines = RichTextParser.Parse(newText);
                foreach (var inline in inlines)
                {
                    textBlock.Inlines.Add(inline);
                }
            }
        }
    }
}
