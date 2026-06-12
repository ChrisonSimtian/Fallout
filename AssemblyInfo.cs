using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Fallout.Application")]
[assembly: InternalsVisibleTo("Fallout.Build.Shared")]
[assembly: InternalsVisibleTo("Fallout.Application.Tests")]
[assembly: InternalsVisibleTo("Fallout.Application.Tools")]
[assembly: InternalsVisibleTo("Fallout.Infrastructure.CI")]
[assembly: InternalsVisibleTo("Fallout.Application.Tools.Tests")]
[assembly: InternalsVisibleTo("Fallout.Infrastructure.CI.Tests")]
[assembly: InternalsVisibleTo("Fallout.Cli")]
[assembly: InternalsVisibleTo("Fallout.Cli.Tests")]
[assembly: InternalsVisibleTo("Fallout.Infrastructure.ProjectModel.Tests")]
[assembly: InternalsVisibleTo("Fallout.SourceGenerators")]
[assembly: InternalsVisibleTo("Fallout.Application.Solutions")]
[assembly: InternalsVisibleTo("Fallout.Infrastructure.Solutions")]
[assembly: InternalsVisibleTo("Fallout.Infrastructure.Solutions.Tests")]
[assembly: InternalsVisibleTo("Fallout.Persistence.Solution")]
[assembly: InternalsVisibleTo("Fallout.Persistence.Solution.Tests")]
[assembly: InternalsVisibleTo("Fallout.Application.Tooling")]
[assembly: InternalsVisibleTo("Fallout.Infrastructure.Tooling")]
[assembly: InternalsVisibleTo("Fallout.Application.Tooling.Tests")]
[assembly: InternalsVisibleTo("Fallout.Infrastructure.Tooling.Tests")]
[assembly: InternalsVisibleTo("Fallout.Core.IO.Globbing")]
[assembly: InternalsVisibleTo("Fallout.Core.Tests")]

// External extensions — kept as Nuke.* until those projects rebrand independently.
[assembly: InternalsVisibleTo("Nuke.VisualStudio")]
[assembly: InternalsVisibleTo("ReSharper.Nuke")]
[assembly: InternalsVisibleTo("ReSharper.Nuke.Rider")]

// External functions — same: outside this repo's rebrand scope.
[assembly: InternalsVisibleTo("Nuke.Remote.Functions")]
[assembly: InternalsVisibleTo("Nuke.Website.Functions")]
