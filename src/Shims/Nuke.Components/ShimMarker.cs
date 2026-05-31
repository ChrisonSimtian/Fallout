// Tells the TransitionShimGenerator to emit shims for every public type whose
// namespace begins with "Fallout.Application.Components" into the corresponding
// "Nuke.Components" namespace. The bulk of this assembly is the component
// interface family (ICompile, IRestore, IPack, ITest, IPublish, IHas*).
// (Source prefix tracks the onion realignment — ADR-0006; the components moved
// into the Application ring, the shim's public Nuke.Components face is unchanged.)

[assembly: Fallout.Migrate.Shims.ShimAllPublicTypesUnder(
    fromNamespacePrefix: "Fallout.Application.Components",
    toNamespacePrefix: "Nuke.Components")]
