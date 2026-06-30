using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace IeltsTeachingAssistant.Helpers
{
    public static class RichTextParser
    {
        // Define colors based on the mockup legend
        private static readonly SolidColorBrush PositiveBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 139, 34)); // ForestGreen
        private static readonly SolidColorBrush LimitationBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 140, 0)); // DarkOrange
        private static readonly SolidColorBrush AccuracyBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 138, 43, 226)); // BlueViolet
        private static readonly SolidColorBrush OrganizationBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 144, 255)); // DodgerBlue

        public static IEnumerable<Inline> Parse(string text)
        {
            var inlines = new List<Inline>();
            if (string.IsNullOrEmpty(text))
                return inlines;

            // Regex to match tags like <positive>word</positive> or just text
            string pattern = @"<(?<tag>\w+)>(?<content>.*?)</\k<tag>>";
            int currentIndex = 0;

            foreach (Match match in Regex.Matches(text, pattern))
            {
                // Add plain text before the match
                if (match.Index > currentIndex)
                {
                    string plainText = text.Substring(currentIndex, match.Index - currentIndex);
                    inlines.Add(new Run { Text = plainText });
                }

                // Create formatted run
                string tag = match.Groups["tag"].Value.ToLower();
                string content = match.Groups["content"].Value;
                
                var run = new Run { Text = content };

                // Apply formatting based on tag
                switch (tag)
                {
                    case "positive":
                        run.Foreground = PositiveBrush;
                        run.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
                        break;
                    case "limitation":
                        run.Foreground = LimitationBrush;
                        run.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
                        break;
                    case "accuracy":
                        run.Foreground = AccuracyBrush;
                        run.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
                        break;
                    case "organization":
                        run.Foreground = OrganizationBrush;
                        run.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
                        break;
                    case "bold":
                        run.FontWeight = FontWeights.Bold;
                        break;
                }

                inlines.Add(run);
                currentIndex = match.Index + match.Length;
            }

            // Add any remaining plain text
            if (currentIndex < text.Length)
            {
                string plainText = text.Substring(currentIndex);
                inlines.Add(new Run { Text = plainText });
            }

            return inlines;
        }
    }
}
