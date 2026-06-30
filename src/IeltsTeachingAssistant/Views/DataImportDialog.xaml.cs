using Microsoft.UI.Xaml.Controls;
using IeltsTeachingAssistant.Services;

namespace IeltsTeachingAssistant.Views;

public sealed partial class DataImportDialog : ContentDialog
{
    public DuplicateStrategy SelectedStrategy
    {
        get
        {
            if (RadioUpdate.IsChecked == true) return DuplicateStrategy.Update;
            if (RadioAllow.IsChecked == true) return DuplicateStrategy.AllowDuplicate;
            return DuplicateStrategy.Skip;
        }
    }

    public DataImportDialog()
    {
        this.InitializeComponent();
    }
}
