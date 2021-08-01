using Microsoft.CodeAnalysis;

namespace HandyIpc.Generator
{
#pragma warning disable RS2008 // Enable analyzer release tracking
    internal static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor HandyIpcNotReferenced = new(
            "HI001",
            "HandyIpc must be referenced",
            "HandyIpc is not referenced. Please add a reference to HandyIpc.",
            "HandyIpc",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor NoInheritance = new(
            "HI002",
            "The contract interface is inheritance-free",
            "The contract interface '{0}' is not allowed to inherit from other interfaces. Consider removing interfaces it inherits from.",
            "HandyIpc",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor MustContainsMethod = new(
            "HI003",
            "The contract interface must contains methods",
            "The interface '{0}' does not contain any methods and no related code will be generated. Consider implementing some methods, or removing the interface, or removing the IpcContractAttribute.",
            "HandyIpc",
            DiagnosticSeverity.Warning,
            true);
    }
#pragma warning restore RS2008 // Enable analyzer release tracking
}
