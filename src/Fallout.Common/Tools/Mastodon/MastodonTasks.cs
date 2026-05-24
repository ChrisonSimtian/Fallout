// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities.Net;

namespace Fallout.Common.Tools.Mastodon;

public static class MastodonTasks
{
    public static void SendMastodonMessage(Configure<MastodonStatus> configurator, string instance, string accessToken)
    {
        SendMastodonMessageAsync(configurator, instance, accessToken).Wait();
    }

    public static async Task SendMastodonMessageAsync(Configure<MastodonStatus> configurator, string instance, string accessToken)
    {
        var status = configurator(new MastodonStatus());
        var uri = new Uri(instance);
        var apiUrl = $"{uri.Scheme}://{uri.Host}/api";
        using var client = new HttpClient();

        async Task<string> PostMediaFile(string file)
        {
            using var stream = File.OpenRead(file);

            var response = await client.CreateRequest(HttpMethod.Post, $"{apiUrl}/v2/media")
                .WithBearerAuthentication(accessToken)
                .WithMultipartFormDataContent(_ => _
                    .AddStreamContent("file", stream, Path.GetFileName(file)))
                .GetResponseAsync();

#pragma warning disable CS0618 // GetBodyAsJson(JObject) retires in v11; migrate to GetBodyAsJsonObject + System.Text.Json.Nodes.JsonObject access patterns then.
            var json = await response.GetBodyAsJson();
#pragma warning restore CS0618
            return json["id"].NotNull().ToString();
        }

        var mediaTasks = status.MediaFiles.Select(PostMediaFile).ToArray();
        Task.WaitAll(mediaTasks);
        var mediaIds = mediaTasks.Select(x => x.Result);

        var response = await client.CreateRequest(HttpMethod.Post, $"{apiUrl}/v1/statuses")
            .WithBearerAuthentication(accessToken)
            .WithJsonContent(
                new
                {
                    status = status.Text,
                    media_ids = mediaIds.ToArray()
                })
            .GetResponseAsync();
        response.AssertSuccessfulStatusCode();
    }
}
