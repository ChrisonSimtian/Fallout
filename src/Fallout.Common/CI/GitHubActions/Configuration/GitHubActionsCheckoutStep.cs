// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using Fallout.Common.Utilities;

namespace Fallout.Common.CI.GitHubActions.Configuration;

public class GitHubActionsCheckoutStep : GitHubActionsStep
{
    public GitHubActionsSubmodules? Submodules { get; set; }
    public bool? Lfs { get; set; }
    public uint? FetchDepth { get; set; }
    public bool? Progress { get; set; }
    public string Filter { get; set; }

    /// <summary>
    /// The git ref to check out. When unset, actions/checkout picks the default for the event
    /// (the merge SHA on pull_request triggers, which leaves HEAD detached). Set to
    /// <c>${{ github.head_ref }}</c> on PR workflows that read <c>.git/HEAD</c> directly
    /// (e.g. <see cref="Fallout.Common.Git.GitRepository.FromLocalDirectory"/>) so the branch
    /// resolves correctly.
    /// </summary>
    public string Ref { get; set; }

    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- uses: actions/checkout@v6");

        if (Submodules.HasValue || Lfs.HasValue || FetchDepth.HasValue || Progress.HasValue ||
            !Filter.IsNullOrWhiteSpace() || !Ref.IsNullOrWhiteSpace())
        {
            using (writer.Indent())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    if (Submodules.HasValue)
                        writer.WriteLine($"submodules: {Submodules.ToString().ToLowerInvariant()}");
                    if(Lfs.HasValue)
                        writer.WriteLine($"lfs: {Lfs.ToString().ToLowerInvariant()}");
                    if (FetchDepth.HasValue)
                        writer.WriteLine($"fetch-depth: {FetchDepth}");
                    if (Progress.HasValue)
                        writer.WriteLine($"progress: {Progress.ToString().ToLowerInvariant()}");
                    if (!Filter.IsNullOrWhiteSpace())
                        writer.WriteLine($"filter: {Filter}");
                    if (!Ref.IsNullOrWhiteSpace())
                        writer.WriteLine($"ref: {Ref}");
                }
            }
        }
    }
}
