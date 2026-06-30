using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Services;
using IeltsTeachingAssistant.ViewModels;

using Microsoft.EntityFrameworkCore.Metadata;

namespace IeltsTeachingAssistant;

/// <summary>
/// Main application entry point. Configures dependency injection,
/// database initialization, and the main window.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the global service provider for dependency injection.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Gets the main application window instance.
    /// </summary>
    public static MainWindow MainWindowInstance { get; private set; } = null!;

    private Window? m_window;

    public App()
    {
        this.InitializeComponent();

        // Configure DI container
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        try
        {
            // Ensure database is created and migrated
            InitializeDatabase();
        }
        catch (Exception ex)
        {
            File.WriteAllText(@"d:\Project\ielts-teaching-assistant\crash.txt", ex.ToString());
        }

        this.UnhandledException += (s, e) =>
        {
            var log = $"Exception: {e.Exception}\nMessage: {e.Exception.Message}\n";
            var inner = e.Exception?.InnerException;
            while (inner != null)
            {
                log += $"Inner Exception: {inner}\nMessage: {inner.Message}\n";
                inner = inner.InnerException;
            }
            File.WriteAllText(@"d:\Project\ielts-teaching-assistant\crash.txt", log);
            e.Handled = true;
        };
    }

    /// <summary>
    /// Registers all services, ViewModels, and data contexts into the DI container.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // ─── Data Layer ───
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IeltsTeachingAssistant",
            "ielts_assistant.db");

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Factory allows ViewModels to create short-lived DbContext instances
        // per async operation, preventing concurrent-access crashes
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"), ServiceLifetime.Singleton);

        // ─── Services ───
        services.AddHttpClient();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddScoped<IVertexAIService, VertexAIService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddScoped<DataImportService>();

        // ─── ViewModels ───
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<IeltsTeachingAssistant.ViewModels.Marking.MainMarkingViewModel>();
        services.AddTransient<ClassManagementViewModel>();
        services.AddTransient<StudentManagementViewModel>();
        services.AddTransient<StudentPerformanceViewModel>();
        services.AddTransient<ClassPerformanceViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<WritingEvaluationViewModel>();
        services.AddTransient<SpeakingEvaluationViewModel>();

        // ─── Logging ───
        services.AddLogging(builder =>
        {
        });
    }

    /// <summary>
    /// Creates the database and applies any pending migrations.
    /// </summary>
    private static void InitializeDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        try
        {
            var connection = context.Database.GetDbConnection();
            bool openedLocal = false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
                openedLocal = true;
            }

            foreach (var entityType in context.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (string.IsNullOrEmpty(tableName)) continue;

                // Ensure the table exists in the SQLite database
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (\"Id\" INTEGER PRIMARY KEY AUTOINCREMENT);";
                    cmd.ExecuteNonQuery();
                }

                var storeObject = StoreObjectIdentifier.Table(tableName, null);

                // Get existing columns for this table
                var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"PRAGMA table_info(\"{tableName}\");";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var colName = reader["name"]?.ToString();
                            if (colName != null)
                            {
                                existingColumns.Add(colName);
                            }
                        }
                    }
                }

                // Check and add missing columns
                foreach (var property in entityType.GetProperties())
                {
                    var columnName = property.GetColumnName(storeObject);
                    if (string.IsNullOrEmpty(columnName)) columnName = property.Name;

                    // Skip primary keys (they must exist if table exists)
                    if (property.IsPrimaryKey()) continue;

                    if (!existingColumns.Contains(columnName))
                    {
                        var storeType = property.GetColumnType(storeObject);
                        if (string.IsNullOrEmpty(storeType)) storeType = property.GetColumnType();

                        var isNullable = property.IsNullable;
                        var nullableSql = isNullable ? "NULL" : "NOT NULL";

                        // Set defaults to avoid SQL errors when adding a non-nullable column to a populated table
                        var defaultSql = "";
                        if (!isNullable)
                        {
                            var clrType = property.ClrType;
                            if (clrType == typeof(string)) defaultSql = " DEFAULT ''";
                            else if (clrType == typeof(int) || clrType == typeof(long) || clrType == typeof(short) || clrType == typeof(byte)) defaultSql = " DEFAULT 0";
                            else if (clrType == typeof(double) || clrType == typeof(float) || clrType == typeof(decimal)) defaultSql = " DEFAULT 0.0";
                            else if (clrType == typeof(bool)) defaultSql = " DEFAULT 0";
                            else if (clrType == typeof(DateTime)) defaultSql = " DEFAULT '0001-01-01 00:00:00'";
                        }

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{columnName}\" {storeType} {nullableSql}{defaultSql};";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            if (openedLocal)
            {
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during automatic migration: {ex}");
        }
    }

    /// <summary>
    /// Called when the application is launched. Creates and activates the main window.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        MainWindowInstance = (MainWindow)m_window;
        m_window.Activate();
    }
}
