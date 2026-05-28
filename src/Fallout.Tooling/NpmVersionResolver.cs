using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Fallout.Common.Tooling;

public static class NpmVersionResolver
{
    private static readonly HttpClient s_client = new();

    public static async Task<string> GetLatestVersion(string packageId)
    {
        try
        {
            var url = $"https://registry.npmjs.org/{packageId}";
            var jsonString = await s_client.GetStringAsync(url);
            var jsonObject = JsonNode.Parse(jsonString)?.AsObject();
            return jsonObject?["dist-tags"]?["latest"]?.GetValue<string>();
        }
        catch
        {
            return null;
        }
    }
}
