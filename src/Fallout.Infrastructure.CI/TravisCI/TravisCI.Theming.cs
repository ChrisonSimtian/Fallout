using System;
using Fallout.Core;

namespace Fallout.Infrastructure.CI.TravisCI;

public partial class TravisCI
{
    protected internal override IDisposable WriteBlock(string text)
    {
        return DelegateDisposable.CreateBracket(
            () => Console.WriteLine($"travis_fold:start:{text}"),
            () => Console.WriteLine($"travis_fold:end:{text}"));
    }
}
