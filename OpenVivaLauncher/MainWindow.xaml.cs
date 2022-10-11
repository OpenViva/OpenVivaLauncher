using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Semver;

namespace OpenVivaLauncher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SemVersion SelectedVersion { get { return ((SemVersion)this.VersionDropDown.SelectedItem); } }

		private bool _versionInstalled = false;

		private readonly GithubService _githubService;
		private readonly GameService _gameService;
		public MainWindow(GithubService github, GameService gameService)
		{
			new StartupWindow().ShowDialog();
			InitializeComponent();

			this._githubService = github;
			this._gameService = gameService;

			_gameService.DownloadProgressChanged += _gameService_DownloadProgressChanged;
			_gameService.DownloadComplete += _gameService_DownloadComplete;
			_gameService.GameProcessExited += _gameService_GameProcessExited;
		}

		private void _gameService_GameProcessExited(object? sender, GameProcessExitedArgs e)
		{
			UpdateLaunchButton();
			this.LaunchButton.IsEnabled = true;
			this.DeleteButton.IsEnabled = true;
		}

		private void _gameService_DownloadComplete(object? sender, GameInstallCompleted e)
		{
			UpdateLaunchButton();
		}

		private void _gameService_DownloadProgressChanged(object? sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			this.ProgressBar.Minimum = 0;
			this.ProgressBar.Maximum = e.TotalBytesToReceive;
			this.ProgressBar.Value = e.BytesReceived;
		}

		private void Window_OnLoaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void Window_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private async void LaunchButton_OnClick(object sender, RoutedEventArgs e)
		{
			if (_versionInstalled)
			{
				this.LaunchButton.Content = "Playing";
				this.LaunchButton.IsEnabled = false;
                this.DeleteButton.IsEnabled = false;
                this.LaunchButton.ToolTip = "You can't launch multiple instances!";
				await this._gameService.StartGameProcess(this.SelectedVersion);
			}
			else
			{
				this.LaunchButton.IsEnabled = false;
				this.VersionDropDown.IsEnabled = false;
				var releases = await _githubService.GetReleasesAsync();
				string versionKey = releases.FirstOrDefault(x => x.Value == SelectedVersion).Key;
				await _gameService.InstallGameVersion(versionKey, releases[versionKey]);
				this.LaunchButton.IsEnabled = true;
				this.VersionDropDown.IsEnabled = true;
			}
		}

		private async void VersionDropDown_OnLoaded(object sender, RoutedEventArgs e)
		{
			var releases = await _githubService.GetReleasesAsync();
			this.VersionDropDown.ItemsSource = releases.Values.ToArray();
			this.VersionDropDown.SelectedIndex = 0;
		}
		private async void VersionDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateLaunchButton();
		}
		private async void Close_OnClick(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}

		private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
		{
			this._gameService.DeleteCurrentVersion(SelectedVersion);
			UpdateLaunchButton();
		}

		private void UpdateLaunchButton()
		{
			if (_gameService.CheckGameInstalled(SelectedVersion.ToString()))
			{
				this.LaunchButton.Content = "Play";
                this.DeleteButton.IsEnabled = true;
                this._versionInstalled = true;
			}
			else
			{
				this.LaunchButton.Content = "Download";
				this.DeleteButton.IsEnabled = false;
				this._versionInstalled = false;
			}
		}

		private void ConfigButton_OnClick(object sender, RoutedEventArgs e)
		{
			new ConfigWindow().ShowDialog();
		}
	}
}
