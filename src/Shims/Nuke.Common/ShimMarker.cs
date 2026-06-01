// Tells the TransitionShimGenerator to emit shims for every public type whose
// namespace begins with "Fallout.Common." into the corresponding "Nuke.Common."
// namespace. The generator walks all referenced Fallout.* assemblies; both
// Fallout.Common and Fallout.Build participate (FalloutBuild itself lives in
// the Fallout.Common namespace despite being declared in the Fallout.Build
// project).

[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Common",
    toNamespacePrefix: "Nuke.Common")]

// The solution-handling types moved from Fallout.Common.ProjectModel to the dedicated Fallout.Solutions
// namespace in v11 (see #248), then split across the onion rings in the 2026 line (ADR-0006, step 5c):
// the Solution/Project model + [Solution] vocabulary → Fallout.Application.Solutions, and the MSBuild
// project evaluator (ProjectModelTasks/ProjectExtensions) → Fallout.Infrastructure.ProjectModel. NUKE's
// Nuke.Common.ProjectModel held both, so mirror both new namespaces back into it for NUKE-era consumers:
// `using Nuke.Common.ProjectModel;` + `[Solution] readonly Solution Solution;` keep compiling.
[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Application.Solutions",
    toNamespacePrefix: "Nuke.Common.ProjectModel")]
[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Infrastructure.ProjectModel",
    toNamespacePrefix: "Nuke.Common.ProjectModel")]
