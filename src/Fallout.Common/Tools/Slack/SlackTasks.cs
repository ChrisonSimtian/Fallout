// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fallout.Common.Tooling;
using Fallout.Common.Utilities;
using Fallout.Common.Utilities.Net;

namespace Fallout.Common.Tools.Slack;

public static class SlackTasks
{
    private static HttpClient s_client = new();

    public static void SendSlackMessage(Configure<SlackMessage> configurator, string webhook)
    {
        SendSlackMessageAsync(configurator, webhook).Wait();
    }

    public static async Task SendSlackMessageAsync(Configure<SlackMessage> configurator, string webhook)
    {
        var message = configurator(new SlackMessage());
        var payload = JsonConvert.SerializeObject(message);

        var response = await s_client.CreateRequest(HttpMethod.Post, webhook)
            .WithFormUrlEncodedContent(new Dictionary<string, string> { ["payload"] = payload })
            .GetResponseAsync();

        var responseText = await response.GetBodyAsync();
        Assert.True(responseText == "ok");
    }

    public static async Task<string> SendOrUpdateSlackMessage(Configure<SlackMessage> configurator, string accessToken)
    {
        var message = configurator(new SlackMessage());

        var response = await s_client.CreateRequest(
                HttpMethod.Post,
                message.Timestamp == null
                    ? "https://slack.com/api/chat.postMessage"
                    : "https://slack.com/api/chat.update")
            .WithBearerAuthentication(accessToken)
            .WithJsonContent(message)
            .GetResponseAsync();

#pragma warning disable CS0618 // GetBodyAsJson(JObject) retires in v11; migrate to GetBodyAsJsonObject + System.Text.Json.Nodes.JsonObject access patterns then.
        var jobject = await response.GetBodyAsJson();
#pragma warning restore CS0618
        var error = jobject.GetPropertyValueOrNull<string>("error");
        Assert.True(error == null, error);

        return jobject.GetPropertyStringValue("ts");
    }
}

[ExcludeFromCodeCoverage]
[Serializable]
public class SlackMessageActionButton : SlackMessageAction
{
    [JsonProperty("type")]
    public string Type => "button";
}
