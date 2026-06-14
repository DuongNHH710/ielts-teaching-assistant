# Developer & Agent Guidelines

Welcome, developer/agent! This file documents key architecture decisions, patterns, and troubleshooting strategies to keep in mind when modifying this repository.

> [!IMPORTANT]
> **Platform Restriction**: This application targets the **Windows x64 architecture only**. Building, deploying, or testing in x86/ARM configurations is disabled. Always run build commands with standard x64 target settings.

---

## 1. WinUI 3 XAML Compiler Tips

### Generic Compiler Failures (`WMC9999`)
- **Symptom**: The XAML compiler throws a generic crash:
  `Xaml Internal Error error WMC9999: Could not find any resources appropriate for the specified culture...`
- **Cause**: This is almost always caused by an unresolved reference or a namespace mapping issue in one of the XAML files.
- **Fix**: Check recent XAML files for:
  - Missing XML namespaces (e.g. `xmlns:views="using:..."`).
  - Mismatched or non-existent static resource names.
  - Invalid method signatures referenced in `x:Bind` expressions (e.g. methods not marked `public` or `static`).

### Interface Binding Issues (`WMC1121`)
- **Symptom**: Binding array variables to interfaces fails compile-time checks:
  `Invalid binding assignment : Cannot directly bind type 'LiveChartsCore.SkiaSharpView.Axis[]' to 'IEnumerable<ICartesianAxis>'`
- **Cause**: WinUI's `x:Bind` compile-time type generator is extremely strict. It cannot implicitly cast array types (like `Axis[]`) to generic interfaces of interfaces (like `IEnumerable<ICartesianAxis>`).
- **Fix**: Define binding properties in ViewModels as `IEnumerable<ICartesianAxis>` and assign them covariant `ICartesianAxis[]` arrays in the initializer.

### String-to-Brush Conversion in Bindings
- **Symptom**: Binding string representations of colors directly to a `Foreground` or `Background` property fails.
- **Cause**: Unlike traditional WPF bindings, WinUI `x:Bind` does not perform implicit type conversions (e.g., parsing a hex `#10B981` string into a `SolidColorBrush`).
- **Fix**: Implement a code-behind helper (e.g., `ConvertHexToBrush(string hex)`) and bind using:
  ```xml
  Foreground="{x:Bind views:MyPage.ConvertHexToBrush(MyColorStringProperty)}"
  ```

---

## 2. Database & Schema Updates

- **Database Framework**: Entity Framework Core with SQLite.
- **Schema Migrations**: The project uses an automatic column-level schema migrator in `App.xaml.cs`'s `InitializeDatabase()`.
- **Constraint**: Since `context.Database.EnsureCreated()` is a no-op on pre-existing database files, any new model fields will not be synced unless the database file is deleted or a custom column ALTER query is executed.
- **Pattern**: If you add new columns to models, register them in the auto-migrator loop in `InitializeDatabase()` to ensure they are added dynamically. Do not rely on standard EF migration commands since users might run on old DB snapshots.

---

## 3. Navigation & State Persistence
- When navigating to performance views, pass the target `studentId` or `classId` as an `int` navigation parameter:
  ```csharp
  Frame.Navigate(typeof(StudentPerformancePage), studentId);
  ```
- Unpack parameters inside the override of `OnNavigatedTo(NavigationEventArgs e)`.
- Use VM-level `IsLoading` booleans to handle asynchronous data fetching safely.
