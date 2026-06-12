using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fallout.Core.IO;

public static class TextTasks
{
    public static UTF8Encoding UTF8NoBom => new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
}
