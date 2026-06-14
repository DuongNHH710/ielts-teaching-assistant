using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IeltsTeachingAssistant.Data;
using IeltsTeachingAssistant.Services;
using IeltsTeachingAssistant.ViewModels;

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
            File.WriteAllText(@"d:\Project\ielts-teaching-assistant\crash.txt", e.Exception.ToString());
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
            options.UseSqlite($"Data Source={dbPath};Journal Mode=WAL;"));

        // ─── Services ───
        services.AddHttpClient();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddScoped<IVertexAIService, VertexAIService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // ─── ViewModels ───
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<SpeakingEvaluationViewModel>();
        services.AddTransient<WritingEvaluationViewModel>();
        services.AddTransient<ClassManagementViewModel>();
        services.AddTransient<StudentManagementViewModel>();
        services.AddTransient<SettingsViewModel>();

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
