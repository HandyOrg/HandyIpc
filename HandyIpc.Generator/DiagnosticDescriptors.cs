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
            "The contract interface ({0}) is not allowed to derive from other interfaces, except for the IDisposable interface",
            "HandyIpc",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor MustContainsMethod = new(
            "HI003",
            "The contract interface must contains methods",
            "The interface ({0}) does not contain any methods and no related code will be generated",
            "HandyIpc",
            DiagnosticSeverity.Warning,
            true);
    }
#pragma warning restore RS2008 // Enable analyzer release tracking
}
