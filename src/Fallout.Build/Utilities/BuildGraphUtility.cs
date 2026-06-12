using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Fallout.Common.Execution;

/// <summary>
/// Serializes a build's target graph — names, descriptions, and all three dependency kinds
/// (execution, order, trigger) — to JSON for consumption by IDE tooling. Unlike
/// <see cref="SchemaUtility"/>, which only enumerates target names for CLI completion,
/// this preserves the edges between targets.
/// </summary>
[Experimental("FALLOUT002")]
public static class BuildGraphUtility
{
    private static readonly JsonSerializerOptions s_writeOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string GetJsonString(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        var targets = new JsonArray();
        foreach (var target in executableTargets.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            targets.Add(new JsonObject
                        {
                            ["name"] = target.Name,
                            ["description"] = target.Description,
                            // Declaring type (e.g. the component interface) — lets IDE tooling resolve
                            // the right source location when several types declare a same-named target.
                            ["declaredIn"] = target.Member?.DeclaringType?.Name,
                            ["default"] = target.IsDefault,
                            ["listed"] = target.Listed,
                            ["dependsOn"] = NameArray(target.ExecutionDependencies),
                            ["after"] = NameArray(target.OrderDependencies),
                            ["triggeredBy"] = NameArray(target.TriggerDependencies),
                            ["triggers"] = NameArray(target.Triggers),
                        });
        }

        var graph = new JsonObject
                    {
                        ["version"] = 1,
                        ["targets"] = targets,
                    };
        return graph.ToJsonString(s_writeOptions) + "\n";
    }

    private static JsonArray NameArray(IEnumerable<ExecutableTarget> targets)
    {
        return new JsonArray(targets
            .Select(x => x.Name)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => (JsonNode)x)
            .ToArray());
    }
}
