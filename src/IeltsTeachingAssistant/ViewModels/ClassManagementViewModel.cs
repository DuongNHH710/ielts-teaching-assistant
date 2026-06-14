using CommunityToolkit.Mvvm.ComponentModel;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.ViewModels;

public partial class ClassManagementViewModel : ObservableObject
{
    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<ClassEntity> _classes = new();

    [ObservableProperty]
    private ClassEntity? _selectedClass;

    private readonly AppDbContext _context;

    public ClassManagementViewModel(AppDbContext context)
    {
        _context = context;
        LoadDataCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadDataAsync);
    }

    public CommunityToolkit.Mvvm.Input.IAsyncRelayCommand LoadDataCommand { get; }

    private async System.Threading.Tasks.Task LoadDataAsync()
    {
        var classes = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(_context.Classes);
        Classes.Clear();
        foreach (var c in classes)
        {
            Classes.Add(c);
        }
    }

    public async System.Threading.Tasks.Task AddClassAsync(ClassEntity newClass)
    {
        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();
        Classes.Add(newClass);
    }
}
