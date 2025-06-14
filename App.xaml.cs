using GitGUI.Core;
using GitGUI.Services;
using GitGUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace GitGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // register your services & VMs
            services.AddSingleton<IGitService, GitLibService>();
            services.AddTransient<OperationViewModel>();
            services.AddTransient<Pages.OperationPage>();

            // register MainWindow so it can take dependencies if you like
            services.AddSingleton<MainWindow>();

            Services = services.BuildServiceProvider();

            // resolve & show
            //var win = Services.GetRequiredService<MainWindow>();
            //win.Show();
            var window = Services.GetRequiredService<MainWindow>();
            window.Show();
            base.OnStartup(e);
        }
    }

}
