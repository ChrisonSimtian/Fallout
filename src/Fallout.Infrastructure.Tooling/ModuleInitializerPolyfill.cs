// ModuleInitializerAttribute ships in the BCL only on .NET 5+. Fallout.Tooling also targets
// netstandard2.0, so polyfill it there (internal) to let ToolingServicesRegistration use [ModuleInitializer]
// across both target frameworks. Standard, well-known shim — the C# compiler recognises any matching type.
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false)]
    internal sealed class ModuleInitializerAttribute : System.Attribute
    {
    }
}
#endif
