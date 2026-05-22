// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

// Tells the TransitionShimGenerator to emit shims for every public type whose
// namespace begins with "Fallout.Build." into the corresponding "Nuke.Build."
// namespace.

[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Build",
    toNamespacePrefix: "Nuke.Build")]
