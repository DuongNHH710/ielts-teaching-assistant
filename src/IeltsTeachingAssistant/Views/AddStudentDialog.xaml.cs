using IeltsTeachingAssistant.Models;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace IeltsTeachingAssistant.Views;

public sealed partial class AddStudentDialog : ContentDialog
{
    public AddStudentDialog(IEnumerable<ClassEntity> availableClasses)
    {
        this.InitializeComponent();
        ClassBox.ItemsSource = availableClasses;
    }

    public Student GetNewStudent()
    {
        float? targetBand = float.TryParse(TargetBandBox.Text, out float tb) ? tb : null;
        var selectedClass = ClassBox.SelectedItem as ClassEntity;

        return new Student
        {
            Name = NameBox.Text,
            Phone = PhoneBox.Text,
            ClassId = selectedClass?.Id ?? 0,
            TargetBandScore = targetBand,
            Notes = NotesBox.Text
        };
    }

    private void Field_Changed(object sender, TextChangedEventArgs e)
    {
        CheckFormComplete();
    }

    private void ClassBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CheckFormComplete();
    }

    private void CheckFormComplete()
    {
        IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(NameBox.Text) && ClassBox.SelectedItem != null;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
    }
}
