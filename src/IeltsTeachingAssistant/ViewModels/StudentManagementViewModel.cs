using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using Microsoft.EntityFrameworkCore;

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

    partial void OnSelectedStudentChanged(Student? value)
    {
        OnPropertyChanged(nameof(SelectedStudentTargetBandScore));
        OnPropertyChanged(nameof(SelectedStudentClassId));
    }

    public double SelectedStudentTargetBandScore
    {
        get => SelectedStudent?.TargetBandScore ?? 0.0;
        set
        {
            if (SelectedStudent != null)
            {
                SelectedStudent.TargetBandScore = value;
                OnPropertyChanged();
            }
        }
    }

    public int SelectedStudentClassId
    {
        get => SelectedStudent?.ClassId ?? 0;
        set
        {
            if (SelectedStudent != null && value > 0 && SelectedStudent.ClassId != value)
            {
                SelectedStudent.ClassId = value;
                OnPropertyChanged();
            }
        }
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

    public async System.Threading.Tasks.Task SaveChangesAsync()
    {
        var selectedId = SelectedStudent?.Id;
        await _context.SaveChangesAsync();
        await LoadDataAsync();
        if (selectedId.HasValue)
        {
            SelectedStudent = Students.FirstOrDefault(s => s.Id == selectedId.Value);
        }
    }

    public async System.Threading.Tasks.Task DeleteStudentAsync(Student student)
    {
        try
        {
            // Reload fresh from DB with all child evaluations
            var toDelete = await _context.Students
                .Include(s => s.SpeakingEvaluations)
                .Include(s => s.WritingEvaluations)
                .Include(s => s.ReadingEvaluations)
                .Include(s => s.ListeningEvaluations)
                .FirstOrDefaultAsync(s => s.Id == student.Id);

            if (toDelete == null) return;

            // Remove child records first (SQLite has no cascade delete configured)
            _context.RemoveRange(toDelete.SpeakingEvaluations);
            _context.RemoveRange(toDelete.WritingEvaluations);
            _context.RemoveRange(toDelete.ReadingEvaluations);
            _context.RemoveRange(toDelete.ListeningEvaluations);

            _context.Students.Remove(toDelete);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DeleteStudentAsync] Error: {ex}");
            throw;
        }

        await LoadDataAsync();
    }
}
