using Newtonsoft.Json;
using Semver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OpenVivaLauncher
{
	public class GithubService
	{
		private const string _releasesEndpoint = "https://api.github.com/repos/OpenViva/OpenViva/releases";
		private HttpClient _httpClient;
		private WebClient _webClient;
		public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
		public event EventHandler<DownloadDataCompletedEventArgs> DownloadComplete;
		public GithubService()
		{
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("OpenVivaLauncher", Assembly.GetExecutingAssembly().GetName().Version.ToString()));
			_webClient = new WebClient();
			_webClient.Headers.Add("User-Agent", $"OpenVivaLauncher-{Assembly.GetExecutingAssembly().GetName().Version}");
			_webClient.DownloadProgressChanged += _webClient_DownloadProgressChanged;
			_webClient.DownloadDataCompleted += _webClient_DownloadDataCompleted;
		}

		private void _webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			DownloadComplete(sender, e);
		}

		private void _webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			DownloadProgressChanged(sender, e);
		}


		public async Task<bool> DownloadVersionAsync(string versionKey, string path = null)
		{
			List<string> assetUrls = await GetAssetUrlsAsync(versionKey);

			foreach (var assetUrl in assetUrls)
			{
				await GetAssetsAsync(assetUrl, path);
			}

			return true;
		}

		public async Task<Dictionary<string, SemVersion>> GetReleasesAsync()
		{
			var pageNumber = "1";
			var releases = new Dictionary<string, SemVersion>();
			while (pageNumber != null)
			{
				var response = await _httpClient.GetAsync(new Uri(_releasesEndpoint + "?page=" + pageNumber));
				var contentJson = await response.Content.ReadAsStringAsync();
				VerifyGitHubAPIResponse(response.StatusCode, contentJson);
				var releasesJson = JsonConvert.DeserializeObject<dynamic>(contentJson);
				foreach (var releaseJson in releasesJson)
				{
					bool preRelease = releaseJson["prerelease"];
					//if (!_settings.IncludePreRelease && preRelease) continue;
					var releaseId = releaseJson["id"].ToString();
					try
					{
						string tagName = releaseJson["tag_name"].ToString();
						var version = CleanVersion(tagName);
						var semVersion = SemVersion.Parse(version);
						releases.Add(releaseId, semVersion);
					}
					catch (Exception)
					{
						// ignored
					}
				}

				pageNumber = GetNextPageNumber(response.Headers);
			}

			return releases;
		}

		private void VerifyGitHubAPIResponse(HttpStatusCode statusCode, string content)
		{
			switch (statusCode)
			{
				case HttpStatusCode.Forbidden when content.Contains("API rate limit exceeded"):
					throw new Exception("GitHub API rate limit exceeded.");
				case HttpStatusCode.NotFound when content.Contains("Not Found"):
					throw new Exception("GitHub Repo not found.");
				default:
					{
						if (statusCode != HttpStatusCode.OK) throw new Exception("GitHub API call failed.");
						break;
					}
			}
		}

		private string GetNextPageNumber(HttpHeaders headers)
		{
			string linkHeader;
			try
			{
				linkHeader = headers.GetValues("Link").FirstOrDefault();
			}
			catch (Exception)
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(linkHeader)) return null;
			var links = linkHeader.Split(',');
			return !links.Any()
				? null
				: (
					from link in links
					where link.Contains(@"rel=""next""")
					select Regex.Match(link, "(?<=page=)(.*)(?=>;)").Value).FirstOrDefault();
		}

		private string CleanVersion(string version)
		{
			var cleanedVersion = version;
			cleanedVersion = cleanedVersion.StartsWith("v") ? cleanedVersion.Substring(1) : cleanedVersion;
			var buildDelimiterIndex = cleanedVersion.LastIndexOf("+", StringComparison.Ordinal);
			cleanedVersion = buildDelimiterIndex > 0
				? cleanedVersion.Substring(0, buildDelimiterIndex)
				: cleanedVersion;
			return cleanedVersion;
		}

		private async Task GetAssetsAsync(string assetUrl, string path)
		{
			using (var md5 = MD5.Create())
			using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
			{
				byte[] data = await _webClient.DownloadDataTaskAsync(assetUrl);

				await fs.WriteAsync(data, 0, data.Length);
				//var response = await _httpClient.GetAsync(assetUrl);
				//Stream release = await response.Content.ReadAsStreamAsync();
				//await release.CopyToAsync(fs);
			}
		}

		private async Task<List<string>> GetAssetUrlsAsync(string releaseId)
		{
			var assets = new List<string>();
			var assetsEndpoint = _releasesEndpoint + "/" + releaseId + "/assets";
			var response = await _httpClient.GetAsync(new Uri(assetsEndpoint));
			var contentJson = await response.Content.ReadAsStringAsync();
			VerifyGitHubAPIResponse(response.StatusCode, contentJson);
			var assetsJson = JsonConvert.DeserializeObject<dynamic>(contentJson);
			foreach (var assetJson in assetsJson)
			{
				assets.Add(assetJson["browser_download_url"].ToString());
			}


			return assets;
		}

		private async Task<bool> VerifyGithubMd5Async(string provided, Stream content)
		{
			using (var md5 = MD5.Create())
			{
				return BitConverter.ToString(await md5.ComputeHashAsync(content)).Replace("-", "").ToLowerInvariant() == provided.ToLowerInvariant();
			}
		}
	}
}