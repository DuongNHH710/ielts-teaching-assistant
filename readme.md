# IELTS Teaching Assistant

An advanced, premium-design AI-powered teaching assistant for IELTS speaking and writing evaluation. Built with WinUI 3 (Windows App SDK) and .NET 8, integrating Google Vertex AI for intelligent feedback, grading, and criteria assessment.

> [!IMPORTANT]
> **Platform Support**: This application is built and optimized for the **Windows x64 platform only**. Other architectures (x86, ARM) are not supported.

---

## Features

- **Class & Student Management**: Track course rosters, private notes, target band scores, and overall progress.
- **AI-Powered Evaluation**:
  - **Speaking Assessment**: Live microphone recordings with automatic transcribing, fluent assessment, and feedback per IELTS criteria.
  - **Writing Assessment**: Essay upload and analysis for Task 1 and Task 2 with grammatical, cohesive, and task-achievement feedback.
- **Performance Dashboards**:
  - **Student View**: Interactive progression trend charts (overall, speaking, writing), criteria progress bars, and master-detail evaluation history with inline transcripts and AI/Teacher comments.
  - **Class View**: Overall class metrics (class average, total evaluations, target reach rate), grade distribution charts, and student performance matrices.
- **Automatic Database Migrations**: Relational schema synchronization and dynamic SQLite updates on application startup.

---

## Technical Stack & Configuration

- **Framework**: WinUI 3 (Windows App SDK 1.5+)
- **Runtime**: .NET 8.0-windows
- **Database**: SQLite with Entity Framework Core (auto-migrated)
- **Charts**: LiveChartsCore.SkiaSharpView.WinUI
- **AI Integration**: Google Cloud Vertex AI Client

### Platform Verification

This project targets the Windows `x64` architecture exclusively. Make sure your build configuration in Visual Studio or CLI is configured as:
```bash
dotnet build -p:Platform=x64
```
Running or building targeting `x86` is disabled.
