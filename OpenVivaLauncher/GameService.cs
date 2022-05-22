using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpCompress;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using SharpCompress.Archives;
using System.Net;
using Semver;
using System.Diagnostics;

namespace OpenVivaLauncher
{
	
	public class GameService
	{

		private readonly GithubService _githubService;
		private Process GameProcess;
		private string GameProcessStdErr = "";

		public GameService(GithubService github)
		{
			_githubService = github;

			_githubService.DownloadProgressChanged += _githubService_DownloadProgressChanged;
			_githubService.DownloadComplete += _githubService_DownloadComplete;
		}

		public event EventHandler<GameInstallCompleted> DownloadComplete; //will probably go unused
		public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
		public event EventHandler<GameProcessExitedArgs> GameProcessExited;

		public string InstallLocation { get; set; } = $"{Directories.Appdata}\\OpenVivaLauncher\\Game\\";

		public bool CheckGameInstalled(string version)
		{
			return File.Exists($"{InstallLocation}{version}\\viva.exe");
		}
		public async Task InstallGameVersion(string version, SemVersion semVer)
		{
			string temp = Path.GetTempFileName();
			await _githubService.DownloadVersionAsync(version, temp);
			DecompressToDirectoryAsync(temp, InstallLocation, semVer);
			File.Delete(temp);
		}
		public async Task StartGameProcess(SemVersion semVer)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			processStartInfo.WorkingDirectory = $"{InstallLocation}{semVer}";
			processStartInfo.FileName = "viva.exe";
			//processStartInfo.RedirectStandardError = true;
			processStartInfo.UseShellExecute = true;

			Process gameProcess = new Process();
			gameProcess.StartInfo = processStartInfo;

			this.GameProcess = gameProcess;
			this.GameProcess.Exited += GameProcess_Exited;
			this.GameProcess.Start();
			await this.GameProcess.WaitForExitAsync();

			EventHandler<GameProcessExitedArgs> handler = GameProcessExited;
			if (handler != null)
				handler(this, new GameProcessExitedArgs(this.GameProcess.ExitCode, this.GameProcessStdErr));
			//List<string> errors = new List<string>();

			//while (!this.GameProcess.HasExited)
			//{
			//	errors.Add(await this.GameProcess.StandardError.ReadLineAsync());
			//}
		}

		public void DeleteCurrentVersion(SemVersion semVer)
		{
			try
			{
				Directory.Delete($"{InstallLocation}{semVer}", true);
			}
			catch (Exception ex)
			{
				new ErrorWindow(ex).ShowDialog();
			}
		}

		private void _githubService_DownloadComplete(object? sender, DownloadDataCompletedEventArgs e)
		{
			//DownloadComplete(this, new GameInstallCompleted(e));
		}

		private void _githubService_DownloadProgressChanged(object? sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			DownloadProgressChanged(sender, e);
		}

		private void DecompressToDirectoryAsync(string path, string target, SemVersion semVer)
		{
			using (var archive = RarArchive.Open(path))
			{
				string root = archive.Entries.First().Key.Split("\\")[0];
				foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
				{
					entry.WriteToDirectory(target, new ExtractionOptions()
					{
						ExtractFullPath = true,
						Overwrite = true,

					});
				}
				Directory.Move($"{target}\\{root}", $"{target}\\{semVer}");
			}
			DownloadComplete(this, new GameInstallCompleted());
		}

		private void GameProcess_Exited(object? sender, EventArgs e)
		{

		}

	}

	public class GameInstallCompleted
	{
		//for implementing download error handling later
	}

	public class GameProcessExitedArgs
	{
		public int ExitCode { get; private set; }
		public string StandardError { get; private set; }
		public GameProcessExitedArgs(int exitCode, string standardError)
		{
			ExitCode = exitCode;
			StandardError = standardError;
		}
	}
}
