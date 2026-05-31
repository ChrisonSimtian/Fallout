using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fallout.Common.Utilities.Net;
using Fallout.Application.Tooling;
using Fallout.Common;

namespace Fallout.Application.Tools.Teams;

public static class TeamsTasks
{
    public static void SendTeamsMessage(Configure<TeamsMessage> configurator, string webhook)
    {
        SendTeamsMessageAsync(configurator, webhook).Wait();
    }

    public static async Task SendTeamsMessageAsync(Configure<TeamsMessage> configurator, string webhook)
    {
        var message = configurator(new TeamsMessage());

        using var client = new HttpClient();

        var response = await client.CreateRequest(HttpMethod.Post, webhook)
            .WithJsonContent(message)
            .GetResponseAsync();

        var responseText = await response.GetBodyAsync();
        Assert.True(responseText == "1", $"'{responseText}' == '1'");
    }
}
