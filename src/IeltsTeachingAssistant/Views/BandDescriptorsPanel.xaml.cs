using Microsoft.UI.Xaml.Controls;
using IeltsTeachingAssistant.Helpers;

namespace IeltsTeachingAssistant.Views;

public sealed partial class BandDescriptorsPanel : UserControl
{
    public BandDescriptorsPanel()
    {
        this.InitializeComponent();
        UpdateDescriptor();
    }

    private void OnCriterionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateDescriptor();
    }

    private void OnBandChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        UpdateDescriptor();
    }

    private void UpdateDescriptor()
    {
        if (CriterionSelector == null || BandSelector == null || DescriptorText == null) return;

        var criterion = (CriterionSelector.SelectedItem as ComboBoxItem)?.Content?.ToString();
        var band = (float)BandSelector.Value;

        if (criterion != null && IeltsBandDescriptors.SpeakingDescriptors.TryGetValue(criterion, out var bandDict))
        {
            if (bandDict.TryGetValue(band, out var text))
            {
                DescriptorText.Text = text;
                return;
            }
        }

        DescriptorText.Text = "Descriptor not available for this selection.";
    }
}
