using NUnit.Framework;
using OpenVivaLauncher;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
	public class Tests
	{
		private GithubService _githubService;
		private string _downloadLocation = Path.GetTempPath() + "OpenViva.rar";

		[SetUp]
		public void Setup()
		{
			_githubService = new GithubService();
		}

		[Test, Order(1), Description("Ensure that we can properly parse github releases without crashing")]
		public async Task GetAllReleases()
		{
			Dictionary<string, Semver.SemVersion> releases;
			Assert.DoesNotThrowAsync(() => _githubService.GetReleasesAsync());
			releases = await _githubService.GetReleasesAsync();
			foreach (KeyValuePair<string, Semver.SemVersion> item in releases)
			{
				TestContext.Out.WriteLine($"Got version - {item.Key}, {item.Value}");
			}
		}

		[Test, Order(2), Description("Ensure that we can properly download github releases without crashing")]
		public async Task DownloadLatestRelease()
		{
			TestContext.Out.WriteLine($"Downloading version to {_downloadLocation}");
			var releases = await _githubService.GetReleasesAsync();
			string[] keys = new string[releases.Count];
			releases.Keys.CopyTo(keys, 0);
			_githubService.DownloadProgressChanged += (sender, e) =>
			{
			};
			_githubService.DownloadComplete += (sender, e) => TestContext.Out.WriteLine("Download Complete.");
			Assert.DoesNotThrowAsync(() => _githubService.DownloadVersionAsync(keys[0], _downloadLocation));
		}


		[TearDown]
		public void TearDown()
		{
			File.Delete(_downloadLocation);
		}
	}
}