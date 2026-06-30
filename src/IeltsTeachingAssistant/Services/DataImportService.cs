using ClosedXML.Excel;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IeltsTeachingAssistant.Services;

public enum DuplicateStrategy
{
    Skip,
    Update,
    AllowDuplicate
}

public class DataImportService
{
    private readonly AppDbContext _context;

    public DataImportService(AppDbContext context)
    {
        _context = context;
    }

    // ── Column layout constants for Classes sheet ─────────────────────────────
    // Keeping them here makes it easy to reorder / add columns without hunting
    // through the entire file.
    private const int C_Name            = 1;
    private const int C_Status          = 2;  // Upcoming | Active | Completed
    private const int C_Mode            = 3;  // Online | Offline
    private const int C_OnlineLink      = 4;  // URL (Online only)
    private const int C_OnlinePass      = 5;  // Meeting password (Online only)
    private const int C_OnlineHostKey   = 6;  // Host key (Online only)
    private const int C_LengthType      = 7;  // "FixedDuration" or "TotalSessions"
    private const int C_Duration        = 8;  // e.g. "2 months" — used when LengthType = FixedDuration
    private const int C_TotalSessions   = 9;  // integer — used when LengthType = TotalSessions
    private const int C_SessionsPerWeek = 10;
    private const int C_TargetBand      = 11;
    private const int C_WeeklySchedule  = 12;
    private const int C_Notes           = 13;

    // ── Column layout constants for Students sheet ────────────────────────────
    private const int S_Name       = 1;
    private const int S_ClassName  = 2;
    private const int S_Phone      = 3;
    private const int S_TargetBand = 4;
    private const int S_Notes      = 5;

    // ─────────────────────────────────────────────────────────────────────────
    // Template generation
    // ─────────────────────────────────────────────────────────────────────────

    public async Task GenerateTemplateAsync(string filePath)
    {
        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();

            BuildClassesSheet(workbook);
            BuildStudentsSheet(workbook);
            BuildInstructionsSheet(workbook);

            workbook.SaveAs(filePath);
        });
    }

    private static void BuildClassesSheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Classes");

        // ── Headers ──────────────────────────────────────────────────────────
        var headers = new[]
        {
            (C_Name,            "Name"),
            (C_Status,          "Status"),
            (C_Mode,            "Mode"),
            (C_OnlineLink,      "OnlineLink"),
            (C_OnlinePass,      "OnlinePass"),
            (C_OnlineHostKey,   "OnlineHostKey"),
            (C_LengthType,      "LengthType"),
            (C_Duration,        "Duration"),
            (C_TotalSessions,   "TotalSessions"),
            (C_SessionsPerWeek, "SessionsPerWeek"),
            (C_TargetBand,      "TargetBandScore"),
            (C_WeeklySchedule,  "WeeklySchedule"),
            (C_Notes,           "Notes"),
        };

        foreach (var (col, label) in headers)
        {
            var cell = ws.Cell(1, col);
            cell.Value = label;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            cell.Style.Font.FontColor       = XLColor.White;
        }

        // ── Sample row 1: Offline class with FIXED DURATION ──────────────────
        ws.Cell(2, C_Name).Value            = "IELTS Intensive 01";
        ws.Cell(2, C_Status).Value          = "Upcoming";
        ws.Cell(2, C_Mode).Value            = "Offline";
        ws.Cell(2, C_OnlineLink).Value      = "";
        ws.Cell(2, C_OnlinePass).Value      = "";
        ws.Cell(2, C_OnlineHostKey).Value   = "";
        ws.Cell(2, C_LengthType).Value      = "FixedDuration";
        ws.Cell(2, C_Duration).Value        = "2 months";
        ws.Cell(2, C_TotalSessions).Value   = "";
        ws.Cell(2, C_SessionsPerWeek).Value = 3;
        ws.Cell(2, C_TargetBand).Value      = 7.0;
        ws.Cell(2, C_WeeklySchedule).Value  = "Mon-Wed-Fri";
        ws.Cell(2, C_Notes).Value           = "Morning class, offline centre";

        // ── Sample row 2: Online class with TOTAL SESSIONS ───────────────────
        ws.Cell(3, C_Name).Value            = "IELTS Flexible Online";
        ws.Cell(3, C_Status).Value          = "Active";
        ws.Cell(3, C_Mode).Value            = "Online";
        ws.Cell(3, C_OnlineLink).Value      = "https://zoom.us/j/123456789";
        ws.Cell(3, C_OnlinePass).Value      = "ielts2024";
        ws.Cell(3, C_OnlineHostKey).Value   = "987654";
        ws.Cell(3, C_LengthType).Value      = "TotalSessions";
        ws.Cell(3, C_Duration).Value        = "";
        ws.Cell(3, C_TotalSessions).Value   = 24;
        ws.Cell(3, C_SessionsPerWeek).Value = 2;
        ws.Cell(3, C_TargetBand).Value      = 6.5;
        ws.Cell(3, C_WeeklySchedule).Value  = "Tue-Thu";
        ws.Cell(3, C_Notes).Value           = "Online via Zoom";

        // ── Drop-down validation hints via cell comments ──────────────────────
        AddComment(ws, 1, C_Status,     "Valid values: Upcoming | Active | Completed");
        AddComment(ws, 1, C_Mode,       "Valid values: Online | Offline");
        AddComment(ws, 1, C_LengthType, "FixedDuration → fill Duration column only\nTotalSessions → fill TotalSessions column only");

        // ── Alternate row shade ───────────────────────────────────────────────
        ws.Row(2).Style.Fill.BackgroundColor = XLColor.FromHtml("#0F172A");
        ws.Row(3).Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");

        ws.Columns().AdjustToContents();
        // Ensure Online columns aren't too narrow
        ws.Column(C_OnlineLink).Width = 35;
    }

    private static void BuildStudentsSheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Students");

        var headers = new[]
        {
            (S_Name,       "Name"),
            (S_ClassName,  "ClassName"),
            (S_Phone,      "Phone"),
            (S_TargetBand, "TargetBandScore"),
            (S_Notes,      "Notes"),
        };

        foreach (var (col, label) in headers)
        {
            var cell = ws.Cell(1, col);
            cell.Value = label;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            cell.Style.Font.FontColor       = XLColor.White;
        }

        ws.Cell(2, S_Name).Value       = "John Doe";
        ws.Cell(2, S_ClassName).Value  = "IELTS Intensive 01";
        ws.Cell(2, S_Phone).Value      = "123456789";
        ws.Cell(2, S_TargetBand).Value = 7.5;
        ws.Cell(2, S_Notes).Value      = "Good at listening";

        ws.Cell(3, S_Name).Value       = "Jane Smith";
        ws.Cell(3, S_ClassName).Value  = "IELTS Flexible Online";
        ws.Cell(3, S_Phone).Value      = "987654321";
        ws.Cell(3, S_TargetBand).Value = 6.5;
        ws.Cell(3, S_Notes).Value      = "Needs writing support";

        ws.Row(2).Style.Fill.BackgroundColor = XLColor.FromHtml("#0F172A");
        ws.Row(3).Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");

        ws.Columns().AdjustToContents();
    }

    private static void BuildInstructionsSheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Instructions");
        ws.Cell(1, 1).Value = "IELTS Teaching Assistant – Data Import Template";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        var lines = new[]
        {
            (3, "CLASSES SHEET"),
            (4, "Name          – Required. Class name. Used as the unique key for matching."),
            (5, "Status        – Optional. One of: Upcoming | Active | Completed  (default: Upcoming)"),
            (6, "Mode          – Optional. One of: Online | Offline  (default: Offline)"),
            (7, "OnlineLink    – URL for the online meeting (Online classes only)"),
            (8, "OnlinePass    – Meeting password (Online classes only)"),
            (9, "OnlineHostKey – Host / moderator key (Online classes only)"),
            (10,"LengthType    – Required. FixedDuration  → fill the Duration column"),
            (11,"                           TotalSessions → fill the TotalSessions column"),
            (12,"Duration      – Used when LengthType = FixedDuration. Free text, e.g. '2 months', '8 weeks'"),
            (13,"TotalSessions – Used when LengthType = TotalSessions. Integer number of sessions."),
            (14,"SessionsPerWeek – How many sessions per week (integer)"),
            (15,"TargetBandScore – Target IELTS band (e.g. 6.5, 7.0)"),
            (16,"WeeklySchedule  – Free text, e.g. 'Mon-Wed-Fri'"),
            (17,"Notes           – Any additional notes"),
            (19,"STUDENTS SHEET"),
            (20,"Name           – Required. Student full name."),
            (21,"ClassName      – Required. Must match an existing class name (from this file or already in the app)."),
            (22,"Phone          – Phone number (text)"),
            (23,"TargetBandScore – Target IELTS band (e.g. 7.5)"),
            (24,"Notes           – Any additional notes"),
        };

        foreach (var (row, text) in lines)
        {
            ws.Cell(row, 1).Value = text;
            if (text.StartsWith("CLASSES") || text.StartsWith("STUDENTS"))
                ws.Cell(row, 1).Style.Font.Bold = true;
        }

        ws.Column(1).Width = 80;
    }

    private static void AddComment(IXLWorksheet ws, int row, int col, string text)
    {
        ws.Cell(row, col).CreateComment().AddText(text);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Import
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<(int ClassesImported, int StudentsImported)> ImportDataAsync(string filePath, DuplicateStrategy strategy)
    {
        int classesImported = 0;
        int studentsImported = 0;

        await Task.Run(async () =>
        {
            using var workbook = new XLWorkbook(filePath);

            // 1. Process Classes
            var classesSheet = workbook.Worksheet("Classes");
            if (classesSheet != null)
            {
                var rangeUsed = classesSheet.RangeUsed();
                if (rangeUsed != null)
                {
                    var rows = rangeUsed.RowsUsed().Skip(1); // Skip header
                    foreach (var row in rows)
                    {
                        string name = row.Cell(C_Name).GetString().Trim();
                        if (string.IsNullOrEmpty(name)) continue;

                        var existingClass = await _context.Classes
                            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

                        if (existingClass != null && strategy == DuplicateStrategy.Skip)
                            continue;

                        if (existingClass != null && strategy == DuplicateStrategy.Update)
                        {
                            ApplyClassRow(row, existingClass);
                            existingClass.UpdatedAt = DateTime.UtcNow;
                            _context.Classes.Update(existingClass);
                            classesImported++;
                        }
                        else // AllowDuplicate or Not Exists
                        {
                            var newClass = new ClassEntity
                            {
                                Name      = name,
                                StartDate = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            ApplyClassRow(row, newClass);
                            _context.Classes.Add(newClass);
                            classesImported++;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // 2. Process Students
            var studentsSheet = workbook.Worksheet("Students");
            if (studentsSheet != null)
            {
                var rangeUsed = studentsSheet.RangeUsed();
                if (rangeUsed != null)
                {
                    var rows = rangeUsed.RowsUsed().Skip(1); // Skip header
                    foreach (var row in rows)
                    {
                        string studentName = row.Cell(S_Name).GetString().Trim();
                        string className   = row.Cell(S_ClassName).GetString().Trim();

                        if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(className)) continue;

                        var classEntity = await _context.Classes
                            .OrderByDescending(c => c.Id)
                            .FirstOrDefaultAsync(c => c.Name.ToLower() == className.ToLower());
                        if (classEntity == null) continue;

                        var existingStudent = await _context.Students
                            .FirstOrDefaultAsync(s => s.Name.ToLower() == studentName.ToLower()
                                                   && s.ClassId == classEntity.Id);

                        if (existingStudent != null && strategy == DuplicateStrategy.Skip)
                            continue;

                        if (existingStudent != null && strategy == DuplicateStrategy.Update)
                        {
                            ApplyStudentRow(row, existingStudent);
                            existingStudent.UpdatedAt = DateTime.UtcNow;
                            _context.Students.Update(existingStudent);
                            studentsImported++;
                        }
                        else
                        {
                            var newStudent = new Student
                            {
                                Name      = studentName,
                                ClassId   = classEntity.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            ApplyStudentRow(row, newStudent);
                            _context.Students.Add(newStudent);
                            studentsImported++;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
        });

        return (classesImported, studentsImported);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Row mappers – separated so Update and Insert share the same logic
    // ─────────────────────────────────────────────────────────────────────────

    private static void ApplyClassRow(IXLRangeRow row, ClassEntity entity)
    {
        // Status
        var statusStr = row.Cell(C_Status).GetString().Trim();
        if (Enum.TryParse<ClassStatus>(statusStr, true, out var status))
            entity.Status = status;

        // Mode (Online / Offline)
        var modeStr = row.Cell(C_Mode).GetString().Trim();
        if (Enum.TryParse<ClassMode>(modeStr, true, out var mode))
            entity.Mode = mode;

        // Online-only fields — clear them if mode is Offline
        if (entity.Mode == ClassMode.Online)
        {
            entity.OnlineLink     = NullIfEmpty(row.Cell(C_OnlineLink).GetString());
            entity.OnlinePassword = NullIfEmpty(row.Cell(C_OnlinePass).GetString());
            entity.OnlineHostKey  = NullIfEmpty(row.Cell(C_OnlineHostKey).GetString());
        }
        else
        {
            entity.OnlineLink     = null;
            entity.OnlinePassword = null;
            entity.OnlineHostKey  = null;
        }

        // Length: either FixedDuration (text) or TotalSessions (integer)
        var lengthType = row.Cell(C_LengthType).GetString().Trim();
        if (lengthType.Equals("TotalSessions", StringComparison.OrdinalIgnoreCase))
        {
            entity.TotalSessions = row.Cell(C_TotalSessions).TryGetValue<int>(out var ts) ? ts : entity.TotalSessions;
            entity.Duration = null; // clear the other field
        }
        else // FixedDuration (default)
        {
            entity.Duration = NullIfEmpty(row.Cell(C_Duration).GetString());
            entity.TotalSessions = 0; // clear the other field
        }

        // Shared fields
        entity.SessionsPerWeek = row.Cell(C_SessionsPerWeek).TryGetValue<int>(out var spw) ? spw : entity.SessionsPerWeek;
        entity.TargetBandScore = row.Cell(C_TargetBand).TryGetValue<double>(out var tb)    ? tb  : entity.TargetBandScore;
        entity.WeeklySchedule  = NullIfEmpty(row.Cell(C_WeeklySchedule).GetString());
        entity.Notes           = NullIfEmpty(row.Cell(C_Notes).GetString());
    }

    private static void ApplyStudentRow(IXLRangeRow row, Student student)
    {
        student.Phone           = NullIfEmpty(row.Cell(S_Phone).GetString());
        student.TargetBandScore = row.Cell(S_TargetBand).TryGetValue<double>(out var tb) ? tb : student.TargetBandScore;
        student.Notes           = NullIfEmpty(row.Cell(S_Notes).GetString());
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
