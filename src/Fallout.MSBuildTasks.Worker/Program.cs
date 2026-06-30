using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fallout.MSBuildTasks.Engine;
using Fallout.MSBuildTasks.Protocol;

// Invoked by the net472 bridge as: <verb> <input.json> <output.json>
// Reads the request, runs the engine, writes a WorkerResponse, and signals via exit code
// (0 = success, 1 = handled failure, 2 = bad invocation).
if (args.Length != 3)
{
    Console.Error.WriteLine("usage: <verb> <input.json> <output.json>");
    return 2;
}

var (verb, inputPath, outputPath) = (args[0], args[1], args[2]);
var messages = new List<string>();
var response = new WorkerResponse();

try
{
    var inputJson = File.ReadAllText(inputPath);
    switch (verb)
    {
        case WorkerVerbs.Codegen:
            var cg = WorkerJson.Deserialize<CodegenRequest>(inputJson);
            CodeGenerationEngine.Generate(
                cg.SpecificationFiles, cg.BaseDirectory, cg.UseNestedNamespaces,
                cg.BaseNamespace, cg.UpdateReferences, messages.Add);
            break;

        case WorkerVerbs.EmbedPackages:
            var ep = WorkerJson.Deserialize<EmbedPackagesRequest>(inputJson);
            response.Files = PackageToolingEngine.GetEmbeddablePackageFiles(ep.ProjectAssetsFile).ToArray();
            break;

        case WorkerVerbs.PackTools:
            var pt = WorkerJson.Deserialize<PackToolsRequest>(inputJson);
            response.ToolFiles = PackageToolingEngine
                .GetPackageToolFiles(pt.ProjectAssetsFile, pt.NuGetPackageRoot, pt.TargetFramework)
                .Select(x => new PackagedToolFileDto { File = x.File, BuildAction = x.BuildAction, PackagePath = x.PackagePath })
                .ToArray();
            break;

        default:
            throw new ArgumentException($"Unknown verb '{verb}'.");
    }

    response.Success = true;
}
catch (Exception exception)
{
    response.Success = false;
    response.Errors = [exception.Message];
}

response.Messages = messages.ToArray();
File.WriteAllText(outputPath, WorkerJson.Serialize(response));
return response.Success ? 0 : 1;
