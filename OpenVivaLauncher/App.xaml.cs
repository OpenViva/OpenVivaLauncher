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
			//globally catch exceptions and display a notification
			Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
			//Autoupdate
			SquirrelAwareApp.HandleEvents(
				onInitialInstall: OnAppInstall,
				onAppUninstall: OnAppUninstall,
				onEveryRun: OnAppRun);

			try
			{
				SquirrelStartup().GetAwaiter().GetResult(); //allow squirrel startup to run async
			}
			catch (Exception ex)
			{
				new ErrorWindow(ex).ShowDialog();
				Environment.Exit(1);
			}

			
			ServiceCollection services = new ServiceCollection();
			ConfigureServices(services);
			_serviceProvider = services.BuildServiceProvider();
		}

		private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			ErrorWindow error = new ErrorWindow(e.Exception);
			error.ShowDialog();
		}

		private async Task SquirrelStartup()
		{
			/*
			Squirrel will throw an error on startup if the launcher was not installed using the squirrel installer
			(which we don't do in development scenarios). Prevent the update system from starting by returning here
			*/
#if DEBUG
			return;
#endif
			//System.Net.Http.HttpRequestException: 'Response status code does not indicate success: 404 (Not Found).'
			try
			{
				string repourl = "https://github.com/OpenViva/OpenVivaLauncher";
				using (var mgr = new GithubUpdateManager(repourl))
				{
					await mgr.UpdateApp();
				}
			}
			catch (System.Net.Http.HttpRequestException httpEx)
			{
				Console.WriteLine("Error in SquirrelStartup");
				Console.WriteLine(httpEx.ToString());
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
