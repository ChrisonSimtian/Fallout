// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Net;

namespace Fallout.Common.CI.GitHubActions;

public partial class GitHubActions
{
    public async Task CreateComment(int issue, string text)
    {
        await _httpClient.Value
            .CreateRequest(HttpMethod.Post, $"repos/{Repository}/issues/{issue}/comments")
            .WithJsonContent(new { body = text })
            .GetResponseAsync();
    }

    private JObject GetJobDetails(long runId)
    {
        var response = _httpClient.Value
            .CreateRequest(HttpMethod.Get, $"repos/{Repository}/actions/runs/{runId}/jobs")
            .GetResponse()
            .AssertSuccessfulStatusCode();

#pragma warning disable CS0618 // GetBodyAsJson(JObject) retires in v11; migrate to GetBodyAsJsonObject + System.Text.Json.Nodes.JsonObject access patterns then.
        return response.GetBodyAsJson().GetAwaiter().GetResult()
#pragma warning restore CS0618
            .GetChildren("jobs")
            .Single(x => x.GetPropertyStringValue("name") == Job);
    }

    private long GetJobId()
    {
        return GetJobDetails(RunId).GetPropertyValue<long>("id");
    }
}
