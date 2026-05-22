// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

// Tells the TransitionShimGenerator to emit shims for every public type whose
// namespace begins with "Fallout.Components" into the corresponding
// "Nuke.Components" namespace. The bulk of this assembly is the component
// interface family (ICompile, IRestore, IPack, ITest, IPublish, IHaz*).

[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Components",
    toNamespacePrefix: "Nuke.Components")]
