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

        public static readonly DiagnosticDescriptor EventWithoutReturn = new(
            "HI003",
            "Events are not allowed to have return values",
            "The event '{0}' cannot have a return value. Consider using an event handler without a return value.",
            HandyIpc,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi003");

        public static readonly DiagnosticDescriptor MustContainsMethod = new(
            "HI100",
            "The contract interface must contains methods",
            "The interface '{0}' does not contain any methods and no related code will be generated. Consider implementing some methods, or removing the interface, or removing the IpcContractAttribute.",
            HandyIpc,
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi100");

        public static readonly DiagnosticDescriptor UseStandardEventHandler = new(
            "HI101",
            "Standard event declarations should be used",
            "The event '{0}' does not use the standard event declaration. Consider using either System.EventHandler or System.EventHandler<T> to declare the event.",
            HandyIpc,
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi101");

        public static readonly DiagnosticDescriptor ContainsNotSupportedMembers = new(
            "HI102",
            "This interface contains members that are not supported",
            "This interface '{0}' contains members that are not supported. Consider removing non-method or non-event members.",
            HandyIpc,
            DiagnosticSeverity.Warning,
            true,
            helpLinkUri: $"{HelpLinkUri}#hi102");
    }
#pragma warning restore RS2008 // Enable analyzer release tracking
}
