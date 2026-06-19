using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.Views;

public sealed partial class StudentManagementPage : Page
{
    public StudentManagementViewModel ViewModel { get; }

    public StudentManagementPage()
    {
        ViewModel = App.Services.GetRequiredService<StudentManagementViewModel>();
        this.InitializeComponent();
        this.Loaded += (s, e) => ViewModel.LoadDataCommand.Execute(null);
    }

    private async void AddStudent_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new AddStudentDialog(ViewModel.AvailableClasses);
        dialog.XamlRoot = this.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var newStudent = dialog.GetNewStudent();
            await ViewModel.AddStudentAsync(newStudent);
        }
    }

    private void ViewPerformance_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SelectedStudent != null)
        {
            Frame.Navigate(typeof(StudentPerformancePage), ViewModel.SelectedStudent.Id);
        }
    }

    public static Visibility DetailVisibility(Student? selectedStudent) => selectedStudent != null ? Visibility.Visible : Visibility.Collapsed;

    private async void SaveStudentChanges_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.SaveChangesAsync();
    }

    private async void DeleteStudent_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SelectedStudent == null) return;

        var confirmDialog = new ContentDialog
        {
            Title = "Confirm deletion",
            Content = $"Are you sure you want to delete the student '{ViewModel.SelectedStudent.Name}'? This will also delete all of their speaking and writing evaluations.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var studentToDelete = ViewModel.SelectedStudent;
            ViewModel.SelectedStudent = null; // Clear selection
            await ViewModel.DeleteStudentAsync(studentToDelete);
        }
    }
}
