using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data;
using System.IO;
using System;
using System.Windows;
using NLog.Config;
using NLog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TourPlanner.DAL;

namespace Tourplanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private static Logger log = LogManager.GetCurrentClassLogger();

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Optional: Directly interact with DbContext to initialize or validate connection
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // You can force initialization here if needed
                dbContext.Database.EnsureCreated();
            }

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("TOURPLANNER_DB_CONNECTION");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string is not defined in environment variables.");
                }
                options.UseNpgsql(connectionString);
            });

            services.AddTransient<MainWindow>(); // Ensure MainWindow can be resolved through DI
        }

        private void InitializeLogger()
        {
            try
            {
                var baseDir = Environment.GetEnvironmentVariable("base_dir");

                if (string.IsNullOrEmpty(baseDir))
                {
                    log.Warn("base_dir environment variable is not set. Using current directory as base directory.");
                    baseDir = AppDomain.CurrentDomain.BaseDirectory;
                }

                var nlogConfigPath = Path.Combine(baseDir, "NLog.config");

                LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);

                log.Info("NLog configuration loaded successfully from: " + nlogConfigPath);

            }
            catch (Exception ex)
            {
                log.Error(ex, "An error occurred while starting the application");
            }
        }
    }
}
