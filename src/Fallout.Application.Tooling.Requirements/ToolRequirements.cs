namespace Fallout.Common.Tooling;

public interface IRequireTool;
public interface IRequireToolWithVersion;


public interface IRequirePathTool : IRequireTool;

public interface IRequireNuGetPackage : IRequireTool, IRequireToolWithVersion;

public interface IRequireNpmPackage : IRequireTool, IRequireToolWithVersion;

public interface IRequireAptGetPackage : IRequireTool;
