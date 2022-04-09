using Microsoft.Extensions.DependencyInjection;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OpenVivaLauncher
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private ServiceProvider _serviceProvider;

		public App()
		{
			//Autoupdate
			SquirrelAwareApp.HandleEvents(
				onInitialInstall: OnAppInstall,
				onAppUninstall: OnAppUninstall,
				onEveryRun: OnAppRun);

			SquirrelStartup().GetAwaiter().GetResult(); //allow squirrel startup to run async
			ServiceCollection services = new ServiceCollection();
			ConfigureServices(services);
			_serviceProvider = services.BuildServiceProvider();

		}

		private async Task SquirrelStartup()
		{
			string repourl = "https://github/Openviva/OpenVivaLauncher";
			using (var mgr = new GithubUpdateManager(repourl))
			{
				await mgr.UpdateApp();
			}
		}

		private void ConfigureServices(ServiceCollection services)
		{
			services.AddTransient<GithubService>();
			services.AddSingleton<MainWindow>();
			services.AddTransient<GameService>();
		}

		private void OnStartup(object sender, StartupEventArgs e)
		{
			var mainWindow = _serviceProvider.GetService<MainWindow>();
			mainWindow.Show();
		}

		private static void OnAppInstall(SemanticVersion version, IAppTools tools)
		{
			tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
		}

		private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
		{
			tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
		}

		private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
		{
			tools.SetProcessAppUserModelId();
			// show a welcome message when the app is first installed
			//if (firstRun) MessageBox.Show("Thanks for installing my application!");
		}
	}
}
