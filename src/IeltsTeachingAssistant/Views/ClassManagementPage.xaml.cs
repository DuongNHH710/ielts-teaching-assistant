using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;
using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.Views;

public sealed partial class ClassManagementPage : Page
{
    public ClassManagementViewModel ViewModel { get; }

    public ClassManagementPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ClassManagementViewModel>();
        this.Loaded += (s, e) => ViewModel.LoadDataCommand.Execute(null);
    }

    private async void AddClass_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new AddClassDialog();
        dialog.XamlRoot = this.XamlRoot;
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            var newClass = dialog.GetNewClass();
            await ViewModel.AddClassAsync(newClass);
        }
    }

    public static Visibility DetailVisibility(ClassEntity? selectedClass) => selectedClass != null ? Visibility.Visible : Visibility.Collapsed;
}
