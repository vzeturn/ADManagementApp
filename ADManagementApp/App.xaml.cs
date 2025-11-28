using ADManagementApp.Models;
using ADManagementApp.Services;
using ADManagementApp.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Windows;

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

            // Bind configuration sections to strongly-typed classes
            services.Configure<ActiveDirectorySettings>(
                configuration.GetSection("ActiveDirectory"));
            services.Configure<ApplicationSettings>(
                configuration.GetSection("Application"));
            services.Configure<SecuritySettings>(
                configuration.GetSection("Security"));

            // Memory Cache
            services.AddMemoryCache();

            // ✅ SECURE CREDENTIAL MANAGEMENT
            services.AddSingleton<ICredentialService, CredentialService>();

            // Core Services with Audit and Validation
            services.AddSingleton<IAuditService, AuditService>();
            services.AddSingleton<IValidationService, ValidationService>();

            // AD Service Chain: Core → Cached → Resilient
            services.AddSingleton<ADService>();
            services.AddSingleton<IADService>(sp =>
            {
                var coreService = sp.GetRequiredService<ADService>();
                var cache = sp.GetRequiredService<IMemoryCache>();
                var logger2 = sp.GetRequiredService<ILogger<CachedADService>>();
                var cachedService = new CachedADService(coreService, cache, logger2);

                var logger3 = sp.GetRequiredService<ILogger<ResilientADService>>();
                return new ResilientADService(cachedService, logger3);
            });

            // UI Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // ✅ ADD SettingsViewModel
            services.AddTransient<SettingsViewModel>();

            // Other ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<GroupManagementViewModel>();

            // Windows
            services.AddTransient<MainWindow>();

            Log.Information("✓ All services configured with secure credential management");
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