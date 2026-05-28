// Tells the TransitionShimGenerator to emit shims for every public type whose
// namespace begins with "Fallout.Build." into the corresponding "Nuke.Build."
// namespace.

[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Build",
    toNamespacePrefix: "Nuke.Build")]
