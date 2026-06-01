using System.Net.Http;
using System.Threading.Tasks;
using Fallout.Application.Tooling;
using Fallout.Kernel.Net;

namespace Fallout.Application.Tools.Discord;

public static class DiscordTasks
{
    public static void SendDiscordMessage(Configure<DiscordMessage> configurator, string webhook)
    {
        SendDiscordMessageAsync(configurator, webhook).Wait();
    }

    public static async Task SendDiscordMessageAsync(Configure<DiscordMessage> configurator, string webhook)
    {
        var message = configurator(new DiscordMessage());

        using var client = new HttpClient();

        var response = await client.CreateRequest(HttpMethod.Post, webhook)
            .WithJsonContent(message)
            .GetResponseAsync();

        response.AssertSuccessfulStatusCode();
    }
}
