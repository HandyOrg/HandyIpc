using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
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
            //Debugger.Launch();

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            Compilation compilation = context.Compilation;

            INamedTypeSymbol? ipcContractAttributeSymbol = compilation.GetTypeByMetadataName("HandyIpc.IpcContractAttribute");
            Extensions.TaskTypeSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
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
                })
                .Select(@interface => (
                    @interface,
                    methods: @interface.GetMembers().OfType<IMethodSymbol>().ToList().AsReadOnly()));

            var fileNameCounter = new Dictionary<string, int>();
            foreach (var (@interface, methods) in contractInterfaces)
            {
                if (@interface.Interfaces.Length > 0)
                {
                    foreach (Location location in @interface.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(NoInheritance, location, @interface.Name));
                    }

                    continue;
                }

                if (!methods.Any())
                {
                    foreach (Location location in @interface.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(MustContainsMethod, location, @interface.Name));
                    }

                    continue;
                }

                string clientProxySource = ClientProxy.Generate(@interface, methods);
                string serverProxySource = ServerProxy.Generate(@interface, methods);
                string dispatcherSource = Dispatcher.Generate(@interface, methods);

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
