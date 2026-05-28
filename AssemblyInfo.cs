using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Fallout.Build")]
[assembly: InternalsVisibleTo("Fallout.Build.Shared")]
[assembly: InternalsVisibleTo("Fallout.Build.Tests")]
[assembly: InternalsVisibleTo("Fallout.Common")]
[assembly: InternalsVisibleTo("Fallout.Common.Tests")]
[assembly: InternalsVisibleTo("Fallout.Cli")]
[assembly: InternalsVisibleTo("Fallout.Cli.Tests")]
[assembly: InternalsVisibleTo("Fallout.ProjectModel.Tests")]
[assembly: InternalsVisibleTo("Fallout.SourceGenerators")]
[assembly: InternalsVisibleTo("Fallout.Solution")]
[assembly: InternalsVisibleTo("Fallout.Solution.Tests")]
[assembly: InternalsVisibleTo("Fallout.Persistence.Solution")]
[assembly: InternalsVisibleTo("Fallout.Persistence.Solution.Tests")]
[assembly: InternalsVisibleTo("Fallout.Tooling")]
[assembly: InternalsVisibleTo("Fallout.Tooling.Tests")]
[assembly: InternalsVisibleTo("Fallout.Utilities.IO.Globbing")]
[assembly: InternalsVisibleTo("Fallout.Utilities.Tests")]

// External extensions — kept as Nuke.* until those projects rebrand independently.
[assembly: InternalsVisibleTo("Nuke.VisualStudio")]
[assembly: InternalsVisibleTo("ReSharper.Nuke")]
[assembly: InternalsVisibleTo("ReSharper.Nuke.Rider")]

// External functions — same: outside this repo's rebrand scope.
[assembly: InternalsVisibleTo("Nuke.Remote.Functions")]
[assembly: InternalsVisibleTo("Nuke.Website.Functions")]
