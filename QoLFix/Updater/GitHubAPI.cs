﻿#if RELEASE_STANDALONE
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QoLFix.Updater
{
    public static partial class UpdateManager
    {
        private static class GitHubAPI
        {
            private static JArray Releases;

            public static async Task<ReleaseInfo> GetLatestRelease(bool includePrerelease = false, bool force = false)
            {
                var tag = default(string);
                try
                {
                    if (!force && Releases != null || await UpdateReleaseObject())
                    {
                        var allowPrerelease = includePrerelease || QoLFixPlugin.Instance.Config.GetConfigEntry<bool>(ConfigNotifyPrerelease).Value;
                        var release = Releases.Children<JObject>().FirstOrDefault(release => !(bool)release["prerelease"] || allowPrerelease);

                        if (release == null) return null;

                        tag = (string)release["tag_name"];
                        if (tag.StartsWith("v")) tag = tag[1..];

                        return new ReleaseInfo
                        {
                            Version = SemVer.Version.Parse(tag),
                            DownloadUrl = (string)release["html_url"],
                            PreRelease = (bool)release["prerelease"],
                        };
                    }
                }
                catch (FormatException ex)
                {
                    throw new FailedUpdateException($"Failed to parse version ({tag})", ex);
                }
                catch (Exception ex)
                {
                    throw new FailedUpdateException("Failed to fetch the latest release version", ex);
                }

                return null;
            }

            private static async Task<bool> UpdateReleaseObject()
            {
                try
                {
                    using var client = new HttpClient()
                    {
                        BaseAddress = new Uri($"https://api.github.com/repos/{QoLFixPlugin.RepoName}/releases"),
                    };
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(QoLFixPlugin.GUID);

                    var res = await client.GetAsync("");
                    if (!res.IsSuccessStatusCode)
                    {
                        LogError($"Failed to fetch release info: {nameof(HttpStatusCode)}.{res.StatusCode}");
                        return false;
                    }

                    var str = await res.Content.ReadAsStringAsync();
                    var arr = JArray.Parse(str);
                    Releases = arr;
                    return true;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to fetch release info: {ex}");
                }

                return false;
            }
        }
    }
}
#endif
