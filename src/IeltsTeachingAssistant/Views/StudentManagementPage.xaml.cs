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
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StudentManagementViewModel>();
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

    public static Visibility DetailVisibility(Student? selectedStudent) => selectedStudent != null ? Visibility.Visible : Visibility.Collapsed;
}
