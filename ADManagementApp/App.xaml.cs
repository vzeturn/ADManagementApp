using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using ADManagementApp.Services;
using ADManagementApp.ViewModels;

namespace ADManagementApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        public IServiceProvider ServiceProvider => _host?.Services
            ?? throw new InvalidOperationException("Service provider not initialized");

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            try
            {
                Log.Information("Starting Active Directory Management Application v2.0");

                // Build host
                _host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                        config.AddJsonFile("appsettings.secure.json", optional: true, reloadOnChange: true);
                        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services, context.Configuration);
                    })
                    .UseSerilog()
                    .Build();

                await _host.StartAsync();

                // Create and show main window
                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();

                Log.Information("Application started successfully with improved service chain");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                MessageBox.Show(
                    $"Failed to start application: {ex.Message}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.AddSingleton(configuration);

            // Memory Cache for CachedADService
            services.AddMemoryCache();

            // Core Services - New Improvements
            services.AddSingleton<ICredentialService, CredentialService>();
            services.AddSingleton<IAuditService, AuditService>();
            services.AddSingleton<IValidationService, ValidationService>();

            // AD Service Chain: Core -> Cached -> Resilient
            // This creates a layered architecture for reliability and performance
            services.AddSingleton<ADService>(); // Core implementation

            services.AddSingleton<IADService>(sp =>
            {
                var logger1 = sp.GetRequiredService<ILogger<ADService>>();
                var coreService = sp.GetRequiredService<ADService>();

                Log.Information("Building AD Service chain: Core -> Cached -> Resilient");

                // Layer 1: Caching for performance
                var cache = sp.GetRequiredService<IMemoryCache>();
                var logger2 = sp.GetRequiredService<ILogger<CachedADService>>();
                var cachedService = new CachedADService(coreService, cache, logger2);
                Log.Information("✓ CachedADService initialized (5-minute cache)");

                // Layer 2: Resilience for reliability
                var logger3 = sp.GetRequiredService<ILogger<ResilientADService>>();
                var resilientService = new ResilientADService(cachedService, logger3);
                Log.Information("✓ ResilientADService initialized (3x retry with backoff)");

                return resilientService;
            });

            // UI Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // ViewModels - Register as Transient to get new instances
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<GroupManagementViewModel>();

            // Windows
            services.AddTransient<MainWindow>();

            Log.Information("✓ All services configured successfully");
            Log.Information("Service Chain: ResilientADService -> CachedADService -> ADService");
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            Log.Information("Application shutting down");

            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}