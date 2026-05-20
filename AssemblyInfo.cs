// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Fallout.Build")]
[assembly: InternalsVisibleTo("Fallout.Build.Shared")]
[assembly: InternalsVisibleTo("Fallout.Build.Tests")]
[assembly: InternalsVisibleTo("Fallout.Common")]
[assembly: InternalsVisibleTo("Fallout.Common.Tests")]
[assembly: InternalsVisibleTo("Fallout.GlobalTool")]
[assembly: InternalsVisibleTo("Fallout.GlobalTool.Tests")]
[assembly: InternalsVisibleTo("Fallout.ProjectModel.Tests")]
[assembly: InternalsVisibleTo("Fallout.SourceGenerators")]
[assembly: InternalsVisibleTo("Fallout.SolutionModel")]
[assembly: InternalsVisibleTo("Fallout.SolutionModel.Tests")]
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
