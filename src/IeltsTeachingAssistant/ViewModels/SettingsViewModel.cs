using CommunityToolkit.Mvvm.ComponentModel;
using IeltsTeachingAssistant.Data;

namespace IeltsTeachingAssistant.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    public SettingsViewModel(AppDbContext context)
    {
        _context = context;
    }
}
