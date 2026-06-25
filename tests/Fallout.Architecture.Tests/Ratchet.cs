using System;
using System.Collections.Generic;
using System.Linq;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Fallout.Architecture.Tests;

/// <summary>
/// Enforces an architecture rule as a <b>one-way ratchet</b> against a documented baseline of known,
/// pre-existing violations (see <see cref="KnownViolations"/>). The architecture here is known to be
/// partially broken; rather than fail the build on day-one debt, each rule is allowed exactly its baseline
/// violations and no more:
/// <list type="bullet">
///   <item>a violation that is <i>not</i> in the baseline fails the test — a new regression sneaking in;</item>
///   <item>a baseline entry that <i>no longer</i> violates also fails the test — the architecture improved, so
///   the stale entry must be deleted to lock the gain in. The baseline can only ever shrink.</item>
/// </list>
/// Pass <see cref="KnownViolations.None"/> for invariants that already hold (most of them) — those are then
/// strict, with zero tolerance.
/// </summary>
internal static class Ratchet
{
    public static void Enforce(IArchRule rule, string because, IReadOnlyCollection<string> baseline)
    {
        var violations = rule.Evaluate(FalloutArchitecture.Architecture)
            .Where(result => !result.Passed)
            .Select(Identify)
            .ToHashSet(StringComparer.Ordinal);

        var baselineSet = baseline.ToHashSet(StringComparer.Ordinal);

        var regressions = violations.Except(baselineSet).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var resolved = baselineSet.Except(violations).OrderBy(x => x, StringComparer.Ordinal).ToList();

        using var scope = new AssertionScope();

        regressions.Should().BeEmpty(
            $"{because}.\nNew architecture violation(s) were introduced that are not in the baseline. " +
            "Fix them, or — if the dependency is genuinely intended — add them to the relevant list in " +
            "KnownViolations with a justifying comment.\nOffenders:\n  " + string.Join("\n  ", regressions));

        resolved.Should().BeEmpty(
            $"{because}.\nThese baseline entries no longer violate, so the architecture improved here. " +
            "Delete them from KnownViolations to lock the gain in — the ratchet only tightens.\nResolved:\n  " +
            string.Join("\n  ", resolved));
    }

    /// <summary>
    /// A stable identifier for a failing evaluation result. Type rules — all of ours — carry the offending
    /// <see cref="IType"/>, whose full name is the baseline key; anything else falls back to ArchUnitNET's own
    /// identifier.
    /// </summary>
    private static string Identify(EvaluationResult result) =>
        result.EvaluatedObject is IType type ? type.FullName : result.EvaluatedObjectIdentifier.ToString();
}
