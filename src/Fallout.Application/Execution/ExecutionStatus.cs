using System.Runtime.CompilerServices;

// Moved to Fallout.Core in v11 (issue #88). Forwarded so existing consumers that reference
// Fallout.Build keep resolving Fallout.Domain.Execution.ExecutionStatus without a recompile.
// The forwarder can be dropped in 12.0 once the plugin SDK is the canonical reference.
[assembly: TypeForwardedTo(typeof(Fallout.Domain.Execution.ExecutionStatus))]
