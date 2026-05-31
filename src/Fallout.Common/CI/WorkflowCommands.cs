using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common.Utilities;

namespace Fallout.Common.CI;

/// <summary>
/// The GitHub-Actions workflow-command dialect — <c>::command parameters::data</c> — shared by every
/// host whose runner speaks it (GitHub Actions and Forgejo Actions today). This is a <em>composition
/// seam</em> (ADR-0005): a host adapter <em>composes</em> this helper rather than inheriting another
/// adapter, so providers that happen to share a wire protocol stay decoupled.
/// </summary>
/// <remarks>
/// The canonical GitHubActions adapter still carries its own copy of this logic; folding it onto this
/// helper is a tracked follow-up (it would touch public API on GitHubActions, so it's kept separate
/// from the Forgejo scaffold to avoid risking that adapter's byte-identical output).
/// </remarks>
internal static class WorkflowCommands
{
    public static void Group(string name) => WriteCommand("group", name);
    public static void EndGroup(string name) => WriteCommand("endgroup", name);
    public static void WriteDebug(string message) => WriteCommand("debug", message);
    public static void WriteWarning(string message) => WriteCommand("warning", message);
    public static void WriteError(string message) => WriteCommand("error", message);

    public static void WriteCommand(string command, string message = null, IReadOnlyDictionary<string, object> parameters = null)
    {
        var formatted = parameters == null || parameters.Count == 0
            ? string.Empty
            : parameters.Select(x => $"{x.Key}={EscapeProperty(x.Value.ToString())}").JoinCommaSpace();

        Console.WriteLine(formatted.IsNullOrEmpty()
            ? $"::{command}::{EscapeData(message)}"
            : $"::{command} {formatted}::{EscapeData(message)}");
    }

    private static string EscapeData(string data)
        => data?.Replace("%", "%25").Replace("\r", "%0D").Replace("\n", "%0A");

    private static string EscapeProperty(string value)
        => value.Replace("%", "%25").Replace("\r", "%0D").Replace("\n", "%0A").Replace(":", "%3A").Replace(",", "%2C");
}
