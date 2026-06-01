
namespace Fallout.Application.CI;

public interface IBuildServer
{
    string Branch { get; }

    string Commit { get; }
}
