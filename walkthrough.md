# Walkthrough: Database Schema, Saving Integrity, and Performance Dashboards

This document outlines the recent design updates, database schema fixes, and charting implementations built into the IELTS Teaching Assistant application.

> [!IMPORTANT]
> **Platform Target**: This project targets the **Windows x64 architecture only**. Building or running the application in `x86` is not supported.

---

## 1. Database Schema and Saving Integrity
- **Dynamic SQLite Migrator**: Added an automatic SQLite dynamic table column migrator inside `InitializeDatabase()` in `App.xaml.cs`. It runs `ALTER TABLE ... ADD COLUMN ...` dynamically on application startup to merge new properties (like the `Prompt` column on `WritingTasks`) without losing existing database entries.
- **Save Session Tracking Fix**: Refactored `SaveSessionAsync()` in speaking and writing ViewModels to use `EntityState.Unchanged` for related navigation models. This stops the Entity Framework change tracker from corrupting foreign key relations during subsequent saves and prevents page locking.

---

## 2. Performance Dashboards for Students and Classes

We designed and built premium performance dashboards using CommunityToolkit MVVM, WinUI 3, and LiveChartsCore.

### Key Visual & Design Features
1. **Dynamic Progress Tracking (x64 Only)**:
   - **Student Dashboard**: Shows an interactive line chart tracking progression of Overall Band, Speaking Band, and Writing Band over time.
   - **Class Dashboard**: Displays a student band score distribution bar chart alongside a class overall average progression line chart.
2. **Criteria Breakdown**:
   - Clean, color-coded visual progress indicators for IELTS criteria (Speaking: Fluency & Coherence, Lexical, Grammar, Pronunciation; Writing: Task Achievement, Coherence & Cohesion, Lexical, Grammar).
3. **Master-Detail Evaluation History**:
   - Provides a comprehensive sidebar of historical tests for the selected student.
   - Clicking an evaluation reveals detailed feedback inline: prompt, transcripts, band scores, and separate sections for **AI Feedback** and **Teacher Feedback** comments.
4. **Student Performance Matrix**:
   - Tabular grid on the Class dashboard highlighting student averages, status badges (e.g. Target Reached / On Track), and direct quick-navigation action buttons.

---

## 3. Build & Type Resolution Errors Resolved
1. **Missing Namespace Mapping**: Added missing `xmlns:views="using:IeltsTeachingAssistant.Views"` namespace imports in the XAML files to resolve calls to static layout visibility helper functions.
2. **Invalid Axis Binding Types**: Solved a WinUI-specific XAML compiler constraint where assigning `Axis[]` directly to `IEnumerable<ICartesianAxis>` triggered compiler error `WMC1121`. Properties on the ViewModels were updated to `IEnumerable<ICartesianAxis>` initialized with covariant `ICartesianAxis[]` arrays.
3. **Hex Color-String Binding Compatibility**: Created a static helper method `ConvertHexToBrush` in `ClassPerformancePage.xaml.cs` to dynamically map status color hex codes to WinUI `Brush` objects in `x:Bind` expressions.
