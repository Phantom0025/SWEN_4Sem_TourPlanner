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
            InitializeLogger();
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureDeleted();
                log.Info("Database deleted successfully.");
                dbContext.Database.EnsureCreated();
                log.Info("Database created successfully.");
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>();
            services.AddTransient<MainWindow>();
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
