using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using IeltsTeachingAssistant.ViewModels;

namespace IeltsTeachingAssistant.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<LoginViewModel>();
        DataContext = ViewModel;
    }
}
