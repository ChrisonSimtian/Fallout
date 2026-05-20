// Copyright 2026 Maintainers of Fallout.
// Originally based on NUKE by Matthias Koch and contributors.
// Distributed under the MIT License.
// https://github.com/ChrisonSimtian/Fallout/blob/main/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Fallout.CodeGeneration;
using Fallout.CodeGeneration.Model;
using Fallout.Common.IO;
using Fallout.Common.Utilities.Collections;

namespace Fallout.MSBuildTasks;

[UsedImplicitly]
public class CodeGenerationTask : ContextAwareTask
{
    [Required]
    public ITaskItem[] SpecificationFiles { get; set; }

    [Required]
    public string BaseDirectory { get; set; }

    public bool UseNestedNamespaces { get; set; }

    [CanBeNull]
    public string BaseNamespace { get; set; }

    public bool UpdateReferences { get; set; }

    protected override bool ExecuteInner()
    {
        var specificationFiles = SpecificationFiles.Select(x => x.GetMetadata("Fullpath")).ToList();

        string GetFilePath(Tool tool)
            => (AbsolutePath) BaseDirectory
               / (UseNestedNamespaces ? tool.Name : ".")
               / tool.DefaultOutputFileName;

        string GetNamespace(Tool tool)
            => !UseNestedNamespaces
                ? BaseNamespace
                : string.IsNullOrEmpty(BaseNamespace)
                    ? tool.Name
                    : $"{BaseNamespace}.{tool.Name}";

        specificationFiles
            .ForEachLazy(x => LogMessage(message: $"Handling {x} ..."))
            .ForEach(x => CodeGenerator.GenerateCode(x, GetFilePath, GetNamespace));

        if (UpdateReferences)
            ReferenceUpdater.UpdateReferences(specificationFiles);

        return true;
    }
}
