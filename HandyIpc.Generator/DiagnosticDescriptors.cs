using Microsoft.CodeAnalysis;

namespace HandyIpc.Generator
{
#pragma warning disable RS2008 // Enable analyzer release tracking
    internal static class DiagnosticDescriptors
    {
        private const string HandyIpc = nameof(HandyIpc);
        private const string HelpLinkUri = "https://github.com/HandyOrg/HandyIpc/wiki/Diagnostic-Messages";

        public static readonly DiagnosticDescriptor HandyIpcNotReferenced = new(
            "HI001",
            "HandyIpc must be referenced",
            "HandyIpc is not referenced. Please add a reference to HandyIpc.",
            HandyIpc,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi001");

        public static readonly DiagnosticDescriptor NoInheritance = new(
            "HI002",
            "The contract interface is inheritance-free",
            "The contract interface '{0}' is not allowed to inherit from other interfaces. Consider removing interfaces it inherits from.",
            HandyIpc,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi002");

        public static readonly DiagnosticDescriptor UseStandardEventHandler = new(
            "HI003",
            "Standard event declarations must be used",
            "The event '{0}' does not use the standard event declaration. Consider using an event signature like 'void EventHandler<T>(object this, T eventArgs)'.",
            HandyIpc,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi003");

        public static readonly DiagnosticDescriptor ContainsNotSupportedMembers = new(
            "HI004",
            "This interface contains members that are not supported",
            "This interface '{0}' contains members that are not supported. Consider removing non-method or non-event members.",
            HandyIpc,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi004");

        public static readonly DiagnosticDescriptor MustContainsMethod = new(
            "HI100",
            "The contract interface must contains methods",
            "The interface '{0}' does not contain any methods and no related code will be generated. Consider implementing some methods, or removing the interface, or removing the IpcContractAttribute.",
            HandyIpc,
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi100");
    }
#pragma warning restore RS2008 // Enable analyzer release tracking
}
