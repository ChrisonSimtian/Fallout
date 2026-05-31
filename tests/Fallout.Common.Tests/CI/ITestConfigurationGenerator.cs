using System;
using System.IO;
using System.Linq;
using Fallout.Application.CI;

namespace Fallout.Common.Tests.CI;

public interface ITestConfigurationGenerator : IConfigurationGenerator
{
    StreamWriter Stream { set; }
}
