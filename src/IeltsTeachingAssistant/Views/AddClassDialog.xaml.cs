using IeltsTeachingAssistant.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace IeltsTeachingAssistant.Views;

public sealed partial class AddClassDialog : ContentDialog
{
    public AddClassDialog()
    {
        this.InitializeComponent();
    }

    public ClassEntity GetNewClass()
    {
        var days = new System.Collections.Generic.List<string>();
        if (MonToggle.IsChecked == true) days.Add("Mon");
        if (TueToggle.IsChecked == true) days.Add("Tue");
        if (WedToggle.IsChecked == true) days.Add("Wed");
        if (ThuToggle.IsChecked == true) days.Add("Thu");
        if (FriToggle.IsChecked == true) days.Add("Fri");
        if (SatToggle.IsChecked == true) days.Add("Sat");
        if (SunToggle.IsChecked == true) days.Add("Sun");

        string time = ClassTimePicker.Time.ToString(@"hh\:mm");
        string weeklySchedule = days.Count > 0 ? $"{string.Join(", ", days)} at {time}" : $"At {time}";

        return new ClassEntity
        {
            Name = NameBox.Text,
            TargetBandScore = double.IsNaN(TargetBandBox.Value) ? 7.5 : TargetBandBox.Value,
            Duration = DurationBox.Text,
            WeeklySchedule = weeklySchedule,
            Status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Active" ? ClassStatus.Active : ClassStatus.Upcoming,
            Mode = IsOnlineBox.IsChecked == true ? ClassMode.Online : ClassMode.Offline,
            OnlineLink = IsOnlineBox.IsChecked == true ? LinkBox.Text : null,
            OnlinePassword = IsOnlineBox.IsChecked == true ? PasswordBox.Text : null,
            OnlineHostKey = IsOnlineBox.IsChecked == true ? HostKeyBox.Text : null
        };
    }

    private void Field_Changed(object sender, TextChangedEventArgs e)
    {
        IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(NameBox.Text);
    }

    private void IsOnlineBox_Changed(object sender, RoutedEventArgs e)
    {
        if (OnlineDetailsPanel != null)
        {
            OnlineDetailsPanel.Visibility = IsOnlineBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Add any validation here if needed
    }
}
