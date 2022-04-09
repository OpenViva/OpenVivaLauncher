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

namespace OpenVivaLauncher
{
	public class GameService
	{
		public string InstallLocation { get; set; } = $"{Directories.Appdata}\\OpenVivaLauncher\\Game\\";

		private readonly GithubService _githubService;

		public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
		public event EventHandler<DownloadDataCompletedEventArgs> DownloadComplete; //will probably go unused
		public GameService(GithubService github)
		{
			_githubService = github;

			_githubService.DownloadProgressChanged += _githubService_DownloadProgressChanged;
			_githubService.DownloadComplete += _githubService_DownloadComplete;
		}

		private void _githubService_DownloadComplete(object? sender, DownloadDataCompletedEventArgs e)
		{
			DownloadComplete(sender, e);
		}

		private void _githubService_DownloadProgressChanged(object? sender, System.Net.DownloadProgressChangedEventArgs e)
		{
			DownloadProgressChanged(sender, e);
		}

		public bool CheckGameInstalled(string version)
		{
			return File.Exists($"{InstallLocation}\\{version}\\viva.exe");
		}

		public async Task InstallGameVersion(string version)
		{
			string temp = Path.GetTempFileName();
			await _githubService.DownloadVersionAsync(version, temp);
			DecompressToDirectoryAsync(temp, InstallLocation);
		}

		private void DecompressToDirectoryAsync(string path, string target)
		{
			using (var archive = RarArchive.Open(path))
			{
				foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
				{
					entry.WriteToDirectory(target, new ExtractionOptions()
					{
						ExtractFullPath = true,
						Overwrite = true,
					});
				}
			}
		}
	}
}
