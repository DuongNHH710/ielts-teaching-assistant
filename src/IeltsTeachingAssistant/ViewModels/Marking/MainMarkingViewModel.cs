using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using IeltsTeachingAssistant.Models.Marking;

namespace IeltsTeachingAssistant.ViewModels.Marking
{
    public partial class MainMarkingViewModel : ObservableObject
    {
        public List<string> Exams { get; } = new() { "IELTS Academic", "IELTS General Training" };
        
        public List<string> Skills { get; } = new() { "Writing Task 1", "Writing Task 2", "Speaking", "Reading", "Listening" };

        [ObservableProperty]
        private string _selectedExam = "IELTS Academic";

        [ObservableProperty]
        private string _selectedSkill = "Writing Task 2";

        [ObservableProperty]
        private object? _currentMarkingContent;

        private readonly SubjectiveGridViewModel _subjectiveViewModel = new();
        private readonly ObjectiveScoreViewModel _objectiveViewModel = new();

        public MainMarkingViewModel()
        {
            _ = LoadContentAsync();
        }

        partial void OnSelectedExamChanged(string value)
        {
            _objectiveViewModel.TrainingType = value.Contains("General") ? "General" : "Academic";
            _ = LoadContentAsync();
        }

        partial void OnSelectedSkillChanged(string value)
        {
            _ = LoadContentAsync();
        }

        private async Task LoadContentAsync()
        {
            if (SelectedSkill == "Reading" || SelectedSkill == "Listening")
            {
                CurrentMarkingContent = _objectiveViewModel;
            }
            else
            {
                // It's a subjective skill
                CurrentMarkingContent = _subjectiveViewModel;
                await LoadSubjectiveDataAsync(SelectedSkill);
            }
        }

        private async Task LoadSubjectiveDataAsync(string skill)
        {
            string fileName = skill switch
            {
                "Writing Task 1" => "WritingTask1.json",
                "Writing Task 2" => "WritingTask2.json",
                "Speaking" => "Speaking.json",
                _ => "WritingTask2.json"
            };

            // Assuming data files are copied to the output directory under Data/Descriptors
            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "Descriptors", fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    var data = JsonSerializer.Deserialize<RubricData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (data != null)
                    {
                        var cellViewModels = new List<CriteriaCellViewModel>();
                        foreach (var cellData in data.Cells)
                        {
                            var cellVm = new CriteriaCellViewModel(cellData.Band, cellData.Criteria);
                            foreach (var desc in cellData.Descriptors)
                            {
                                cellVm.Descriptors.Add(new DescriptorViewModel(desc));
                            }
                            cellViewModels.Add(cellVm);
                        }

                        _subjectiveViewModel.InitializeGrid(data.Criteria, data.Bands, cellViewModels);
                    }
                }
                catch (Exception ex)
                {
                    // Handle load error appropriately
                    System.Diagnostics.Debug.WriteLine($"Error loading rubric: {ex.Message}");
                }
            }
            else
            {
                // Fallback for missing file or provide dummy data
                System.Diagnostics.Debug.WriteLine($"Rubric file not found: {filePath}");
                
                // Let's create dummy data for Writing Task 2 if file not found, so UI still shows something
                if (skill == "Writing Task 2")
                {
                    var dummyCriteria = new[] { "Task response", "Coherence and cohesion", "Lexical resource", "Grammatical range and accuracy" };
                    var dummyBands = new[] { 9, 8, 7 };
                    var dummyCells = new List<CriteriaCellViewModel>();
                    foreach(var b in dummyBands)
                    {
                        foreach(var c in dummyCriteria)
                        {
                            var cell = new CriteriaCellViewModel(b, c);
                            cell.Descriptors.Add(new DescriptorViewModel($"Sample descriptor for {c} Band {b}"));
                            dummyCells.Add(cell);
                        }
                    }
                    _subjectiveViewModel.InitializeGrid(dummyCriteria, dummyBands, dummyCells);
                }
            }
        }
    }
}
