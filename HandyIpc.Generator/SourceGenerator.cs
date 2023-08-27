using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static HandyIpc.Generator.DiagnosticDescriptors;

namespace HandyIpc.Generator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            Compilation compilation = context.Compilation;
            Extensions.Initialize(compilation);

            INamedTypeSymbol? ipcContractAttributeSymbol = compilation.GetTypeByMetadataName("HandyIpc.IpcContractAttribute");
            if (ipcContractAttributeSymbol is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(HandyIpcNotReferenced, Location.None));
                return;
            }

            var contractInterfaces = receiver.CandidateInterfaces
                .GroupBy(@interface => @interface.SyntaxTree)
                .SelectMany(group =>
                {
                    SemanticModel model = compilation.GetSemanticModel(group.Key);
                    return group
                        .Select(@interface => model.GetDeclaredSymbol(@interface))
                        .Where(@interface => @interface is not null)
                        // WORKAROUND: The @interface must not be null here.
                        .Select(@interface => @interface!)
                        .Where(@interface => ContainsAttribute(@interface, ipcContractAttributeSymbol));
                });

            var fileNameCounter = new Dictionary<string, int>();
            foreach (var @interface in contractInterfaces)
            {
                ISymbol[] members = @interface.GetMembers().ToArray();
                IMethodSymbol[] methods = members.OfType<IMethodSymbol>()
                    .Where(item => item.MethodKind
                        is not MethodKind.EventAdd
                        and not MethodKind.EventRemove
                        and not MethodKind.EventRaise)
                    .ToArray();
                IEventSymbol[] events = members.OfType<IEventSymbol>().ToArray();

                if (members.Length != methods.Length + events.Length * 3)
                {
                    foreach (Location location in @interface.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ContainsNotSupportedMembers, location, @interface.Name));
                    }

                    continue;
                }

                if (@interface.Interfaces.Length > 0)
                {
                    foreach (Location location in @interface.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(NoInheritance, location, @interface.Name));
                    }

                    continue;
                }

                if (!methods.Any() && !events.Any())
                {
                    foreach (Location location in @interface.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(MustContainsMethod, location, @interface.Name));
                    }

                    continue;
                }

                bool hasInvalidEvent = false;
                foreach (var location in events.Where(@event => !@event.IsStdEventHandler()).SelectMany(@event => @event.Locations))
                {
                    hasInvalidEvent = true;
                    context.ReportDiagnostic(Diagnostic.Create(UseStandardEventHandler, location, @interface.Name));
                }

                if (hasInvalidEvent)
                {
                    continue;
                }

                string clientProxySource = ClientProxy.Generate(@interface, methods, events);
                string serverProxySource = ServerProxy.Generate(@interface, methods, events);
                string dispatcherSource = Dispatcher.Generate(@interface, methods, events);

                string fileName = GetUniqueString(@interface.Name, fileNameCounter);

                context.AddSource($"{fileName}.ClientProxy.g.cs", SourceText.From(clientProxySource, Encoding.UTF8));
                context.AddSource($"{fileName}.ServerProxy.g.cs", SourceText.From(serverProxySource, Encoding.UTF8));
                context.AddSource($"{fileName}.Dispatcher.g.cs", SourceText.From(dispatcherSource, Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public static bool ContainsAttribute(INamedTypeSymbol symbol, INamedTypeSymbol attributeSymbol)
        {
            return symbol
                .GetAttributes()
                .Any(attribute => attribute.AttributeClass is not null &&
                                  attribute.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
        }

        public static string GetUniqueString(string template, IDictionary<string, int> counter)
        {
            if (counter.TryGetValue(template, out int count))
            {
                return $"{template}{++count}";
            }

            counter[template] = count;
            return template;
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<InterfaceDeclarationSyntax> CandidateInterfaces { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is InterfaceDeclarationSyntax { AttributeLists: { Count: > 0 } } interfaceDeclarationSyntax)
                {
                    CandidateInterfaces.Add(interfaceDeclarationSyntax);
                }
            }
        }
    }
}
