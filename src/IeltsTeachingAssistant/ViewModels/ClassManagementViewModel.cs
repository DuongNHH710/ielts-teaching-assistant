using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;

namespace IeltsTeachingAssistant.ViewModels;

public partial class ClassManagementViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ClassEntity> _classes = new();

    [ObservableProperty]
    private ClassEntity? _selectedClass;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private int _selectedTab = 0; // 0 = Ongoing/Active, 1 = To Be Opened/Upcoming, 2 = Ended/Completed

    [ObservableProperty]
    private string _sortBy = "Name";

    [ObservableProperty]
    private ObservableCollection<ClassEntity> _filteredClasses = new();

    private readonly AppDbContext _context;

    public ClassManagementViewModel(AppDbContext context)
    {
        _context = context;
        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
    }

    partial void OnSearchQueryChanged(string value) => UpdateFilteredClasses();
    partial void OnSelectedTabChanged(int value) => UpdateFilteredClasses();
    partial void OnSortByChanged(string value) => UpdateFilteredClasses();

    partial void OnSelectedClassChanged(ClassEntity? value)
    {
        OnPropertyChanged(nameof(SelectedClassStatus));
        OnPropertyChanged(nameof(SelectedClassMode));
        OnPropertyChanged(nameof(SelectedClassTotalSessions));
        OnPropertyChanged(nameof(SelectedClassSessionsCompleted));
        OnPropertyChanged(nameof(SelectedClassProgress));
        OnPropertyChanged(nameof(SelectedClassProgressString));
        OnPropertyChanged(nameof(SelectedClassTargetBand));
    }

    public string[] StatusOptions { get; } = new[] { "Upcoming", "Active", "Completed" };
    public string[] ModeOptions { get; } = new[] { "Online", "Offline" };

    public string SelectedClassStatus
    {
        get => SelectedClass?.Status.ToString() ?? "Upcoming";
        set
        {
            if (SelectedClass != null && Enum.TryParse<ClassStatus>(value, out var status))
            {
                if (SelectedClass.Status != status)
                {
                    SelectedClass.Status = status;
                    OnPropertyChanged();
                    UpdateFilteredClasses();
                }
            }
        }
    }

    public string SelectedClassMode
    {
        get => SelectedClass?.Mode.ToString() ?? "Offline";
        set
        {
            if (SelectedClass != null && Enum.TryParse<ClassMode>(value, out var mode))
            {
                if (SelectedClass.Mode != mode)
                {
                    SelectedClass.Mode = mode;
                    OnPropertyChanged();
                }
            }
        }
    }

    public double SelectedClassTotalSessions
    {
        get => SelectedClass?.TotalSessions ?? 0;
        set
        {
            if (SelectedClass != null)
            {
                int val = (int)value;
                if (SelectedClass.TotalSessions != val)
                {
                    SelectedClass.TotalSessions = val;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedClassProgress));
                    OnPropertyChanged(nameof(SelectedClassProgressString));
                }
            }
        }
    }

    public double SelectedClassSessionsCompleted
    {
        get => SelectedClass?.SessionsCompleted ?? 0;
        set
        {
            if (SelectedClass != null)
            {
                int val = (int)value;
                if (SelectedClass.SessionsCompleted != val)
                {
                    SelectedClass.SessionsCompleted = val;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedClassProgress));
                    OnPropertyChanged(nameof(SelectedClassProgressString));
                }
            }
        }
    }

    public float SelectedClassProgress => SelectedClass?.Progress ?? 0;

    public string SelectedClassProgressString => SelectedClass != null
        ? $"{SelectedClass.SessionsCompleted} / {SelectedClass.TotalSessions} sessions ({(int)SelectedClass.Progress}%)"
        : "0 / 0 sessions (0%)";

    public double SelectedClassTargetBand
    {
        get => SelectedClass?.TargetBandScore ?? 7.5;
        set
        {
            if (SelectedClass != null)
            {
                if (SelectedClass.TargetBandScore != value)
                {
                    SelectedClass.TargetBandScore = value;
                    OnPropertyChanged();
                    UpdateFilteredClasses();
                }
            }
        }
    }

    public IAsyncRelayCommand LoadDataCommand { get; }

    public void UpdateFilteredClasses()
    {
        var query = Classes.AsEnumerable();

        // 1. Filter by Status (SelectedTab)
        var targetStatus = SelectedTab switch
        {
            1 => ClassStatus.Upcoming,
            2 => ClassStatus.Completed,
            _ => ClassStatus.Active
        };
        query = query.Where(c => c.Status == targetStatus);

        // 2. Filter by SearchQuery
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var searchLower = SearchQuery.ToLowerInvariant();
            query = query.Where(c => c.Name.ToLowerInvariant().Contains(searchLower));
        }

        // 3. Sort
        query = SortBy switch
        {
            "Progress" => query.OrderByDescending(c => c.Progress),
            "OverallBand" => query.OrderByDescending(c => c.OverallBand),
            "StartDate" => query.OrderBy(c => c.StartDate),
            _ => query.OrderBy(c => c.Name)
        };

        FilteredClasses.Clear();
        foreach (var c in query)
        {
            FilteredClasses.Add(c);
        }
    }

    private async Task LoadDataAsync()
    {
        var classes = await _context.Classes
            .Include(c => c.Students)
                .ThenInclude(s => s.SpeakingEvaluations)
            .Include(c => c.Students)
                .ThenInclude(s => s.WritingEvaluations)
            .Include(c => c.Students)
                .ThenInclude(s => s.ReadingEvaluations)
            .Include(c => c.Students)
                .ThenInclude(s => s.ListeningEvaluations)
            .ToListAsync();

        Classes.Clear();
        foreach (var c in classes)
        {
            Classes.Add(c);
        }
        UpdateFilteredClasses();
    }

    public async Task AddClassAsync(ClassEntity newClass)
    {
        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();
        Classes.Add(newClass);
        UpdateFilteredClasses();
    }

    public async Task SaveChangesAsync()
    {
        var selectedId = SelectedClass?.Id;
        await _context.SaveChangesAsync();
        await LoadDataAsync();
        if (selectedId.HasValue)
        {
            SelectedClass = Classes.FirstOrDefault(c => c.Id == selectedId.Value);
        }
    }

    public async Task DeleteClassAsync(ClassEntity classEntity)
    {
        _context.Classes.Remove(classEntity);
        await _context.SaveChangesAsync();
        await LoadDataAsync();
    }
}

