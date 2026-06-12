
namespace Fallout.Core;

public static partial class StringExtensions
{
    public static string Indent(this string text, int count)
    {
        return ' '.Repeat(count) + text;
    }
}
