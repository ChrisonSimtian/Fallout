// Tells the TransitionShimGenerator to emit shims for every public type whose
// namespace begins with "Fallout.Common." into the corresponding "Nuke.Common."
// namespace. The generator walks all referenced Fallout.* assemblies; both
// Fallout.Common and Fallout.Build participate (FalloutBuild itself lives in
// the Fallout.Common namespace despite being declared in the Fallout.Build
// project).

[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Common",
    toNamespacePrefix: "Nuke.Common")]

// The solution-handling types moved from Fallout.Common.ProjectModel to the
// dedicated Fallout.Solutions namespace in v11 (see #248 and the broader
// onion-layering work). For NUKE-era consumers, mirror them into the legacy
// Nuke.Common.ProjectModel namespace so existing `using Nuke.Common.ProjectModel;`
// + `[Solution] readonly Solution Solution;` keep compiling.
[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Solutions",
    toNamespacePrefix: "Nuke.Common.ProjectModel")]
