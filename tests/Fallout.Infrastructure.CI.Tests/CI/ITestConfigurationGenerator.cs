using System;
using System.IO;
using System.Linq;
using Fallout.Application.CI;

namespace Fallout.Infrastructure.CI.Tests;

public interface ITestConfigurationGenerator : IConfigurationGenerator
{
    StreamWriter Stream { set; }
}
