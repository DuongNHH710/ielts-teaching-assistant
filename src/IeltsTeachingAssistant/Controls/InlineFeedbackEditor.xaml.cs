using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace IeltsTeachingAssistant.Controls;

public sealed partial class InlineFeedbackEditor : UserControl
{
    public static readonly DependencyProperty RtfTextProperty =
        DependencyProperty.Register(
            nameof(RtfText),
            typeof(string),
            typeof(InlineFeedbackEditor),
            new PropertyMetadata(string.Empty, OnRtfTextChanged));

    public string RtfText
    {
        get => (string)GetValue(RtfTextProperty);
        set => SetValue(RtfTextProperty, value);
    }

    private bool _isInternalChange;

    public InlineFeedbackEditor()
    {
        this.InitializeComponent();
    }

    private static void OnRtfTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InlineFeedbackEditor editor && !editor._isInternalChange)
        {
            var newText = e.NewValue as string;
            editor.EditorBox.Document.SetText(TextSetOptions.FormatRtf, newText ?? string.Empty);
        }
    }

    private void EditorBox_LostFocus(object sender, RoutedEventArgs e)
    {
        UpdateRtfTextProperty();
    }

    private void UpdateRtfTextProperty()
    {
        _isInternalChange = true;
        EditorBox.Document.GetText(TextGetOptions.FormatRtf, out var text);
        RtfText = text;
        _isInternalChange = false;
    }

    private void BoldButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = EditorBox.Document.Selection;
        if (selection != null)
        {
            selection.CharacterFormat.Bold = selection.CharacterFormat.Bold == FormatEffect.On ? FormatEffect.Off : FormatEffect.On;
            UpdateRtfTextProperty();
        }
    }

    private void ItalicButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = EditorBox.Document.Selection;
        if (selection != null)
        {
            selection.CharacterFormat.Italic = selection.CharacterFormat.Italic == FormatEffect.On ? FormatEffect.Off : FormatEffect.On;
            UpdateRtfTextProperty();
        }
    }

    private void UnderlineButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = EditorBox.Document.Selection;
        if (selection != null)
        {
            selection.CharacterFormat.Underline = selection.CharacterFormat.Underline == UnderlineType.Single ? UnderlineType.None : UnderlineType.Single;
            UpdateRtfTextProperty();
        }
    }

    private void RemoveFormatButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = EditorBox.Document.Selection;
        if (selection != null)
        {
            selection.CharacterFormat.Bold = FormatEffect.Off;
            selection.CharacterFormat.Italic = FormatEffect.Off;
            selection.CharacterFormat.Underline = UnderlineType.None;
            selection.CharacterFormat.ForegroundColor = Microsoft.UI.Colors.Black;
            UpdateRtfTextProperty();
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        EditorBox.Document.SetText(TextSetOptions.None, string.Empty);
        UpdateRtfTextProperty();
    }

    private void TagButton_Click(object sender, RoutedEventArgs e)
    {
        var selection = EditorBox.Document.Selection;
        if (selection != null && sender is ToggleButton btn)
        {
            var tag = btn.Tag as string;
            
            // For simplicity, just changing color based on tag for now.
            // In a real app we might use custom text metadata or insert a comment block.
            Windows.UI.Color color = Microsoft.UI.Colors.Black;
            switch(tag)
            {
                case "Grammar": color = Microsoft.UI.ColorHelper.FromArgb(255, 59, 130, 246); break; // #3B82F6
                case "Vocabulary": color = Microsoft.UI.ColorHelper.FromArgb(255, 217, 119, 6); break; // #D97706
                case "Spelling": color = Microsoft.UI.ColorHelper.FromArgb(255, 180, 83, 9); break; // #B45309
                case "Coherence": color = Microsoft.UI.ColorHelper.FromArgb(255, 139, 92, 246); break; // #8B5CF6
                case "Task": color = Microsoft.UI.ColorHelper.FromArgb(255, 13, 148, 136); break; // #0D9488
                case "Style": color = Microsoft.UI.ColorHelper.FromArgb(255, 75, 85, 99); break; // #4B5563
                case "Idea": color = Microsoft.UI.ColorHelper.FromArgb(255, 217, 70, 239); break; // #D946EF
            }

            selection.CharacterFormat.ForegroundColor = color;
            UpdateRtfTextProperty();
            
            // Uncheck the button immediately to act like a normal button but styled as a ToggleButton
            btn.IsChecked = false;
        }
    }
}
