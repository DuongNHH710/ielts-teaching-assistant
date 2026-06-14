using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.ViewModels;

public partial class StudentManagementViewModel : ObservableObject
{
    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<Student> _students = new();

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<ClassEntity> _availableClasses = new();

    [ObservableProperty]
    private Student? _selectedStudent;

    private readonly AppDbContext _context;

    public StudentManagementViewModel(AppDbContext context)
    {
        _context = context;
        LoadDataCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadDataAsync);
    }

    public CommunityToolkit.Mvvm.Input.IAsyncRelayCommand LoadDataCommand { get; }

    private async System.Threading.Tasks.Task LoadDataAsync()
    {
        var students = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(
                Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(
                    Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(_context.Students, s => s.Class), 
                    s => s.SpeakingEvaluations),
                s => s.WritingEvaluations));
                
        Students.Clear();
        foreach (var s in students)
        {
            Students.Add(s);
        }

        var classes = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(_context.Classes);
        AvailableClasses.Clear();
        foreach (var c in classes)
        {
            AvailableClasses.Add(c);
        }
    }

    public async System.Threading.Tasks.Task AddStudentAsync(Student newStudent)
    {
        _context.Students.Add(newStudent);
        await _context.SaveChangesAsync();
        
        // Reload to get relationships populated
        await LoadDataAsync();
    }
}
