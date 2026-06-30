using System.Text.Json.Serialization;

using Fallout.Common;
namespace Fallout.Application.Tools.Notifications.Teams;

public partial class TeamsMessage
{
    [JsonPropertyName("@type")]
    internal string Type => "MessageCard";
    [JsonPropertyName("@context")]
    internal string Context => "http://schema.org/extensions";
}
