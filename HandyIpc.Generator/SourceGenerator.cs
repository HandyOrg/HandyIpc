using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HandyIpc.Generator.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HandyIpc.Generator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private const string ClientsFileName = "ClientProxies.g.cs";
        private const string ServerProxiesFileName = "ServerProxies.g.cs";
        private const string DispatchersFileName = "Dispatchers.g.cs";

        public void Execute(GeneratorExecutionContext context)
        {
            //Debugger.Launch();

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var compilation = context.Compilation;

            var ipcContractAttributeSymbol = compilation.GetTypeByMetadataName("HandyIpc.IpcContractAttribute");
            if (ipcContractAttributeSymbol is null)
            {
                // TODO: context.ReportDiagnostic(...)
                return;
            }

            var ipcContractInterfaces = SelectIpcContractInterfaces(receiver.CandidateInterfaces)
                .OrderBy(i => i.Identifier.Text)
                .ToList();
            var usingList = ipcContractInterfaces
                .SelectMany(@interface => @interface
                    .GetSyntaxNodeRoot<SyntaxNode>()!
                    .DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(x => $"{x.Alias} {x.StaticKeyword} {x.Name}".TrimStart()))
                .Distinct()
                .Where(item => item != "HandyIpc")
                .ToList();
            usingList.AddIfMissing("System.Threading.Tasks");

            var classList = ipcContractInterfaces.Select(GetClassData).ToList();
            if (classList.Any(classData => classData.HasGenericMethod))
            {
                usingList.AddIfMissing("System.Collections.Generic");
                usingList.AddIfMissing("System.Reflection");
            }

            var data = new TemplateFileData(usingList, classList);

            context.AddSource(ClientsFileName, SourceText.From(ClientProxy.Generate(data), Encoding.UTF8));
            context.AddSource(ServerProxiesFileName, SourceText.From(ServerProxy.Generate(data), Encoding.UTF8));
            context.AddSource(DispatchersFileName, SourceText.From(Dispatcher.Generate(data), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private static ClassData GetClassData(InterfaceDeclarationSyntax @interface)
        {
            var result = new ClassData
            {
                InterfaceName = @interface.GetInterfaceName()
            };
            // Resolve nested interface
            result.GeneratedClassSuffix = result.InterfaceName.Replace(".", string.Empty);
            var parent = @interface.GetSyntaxNodeRoot<NamespaceDeclarationSyntax>();
            // FIXME: Support interfaces without namespace.
            result.Namespace = parent?.Name.ToString() ?? $"HandyIpc{result.GeneratedClassSuffix}";

            // Resolve generic parameters of this ipc interface.
            if (@interface.TypeParameterList is not null)
            {
                var typeParameters = @interface.TypeParameterList.Parameters;
                if (typeParameters.Any())
                {
                    result.TypeParameters = typeParameters.Select(item => item.Identifier.ValueText).ToList();
                }

                result.ConstraintClauses = @interface.ConstraintClauses.ToFullString().Trim();
            }

            result.MethodList = @interface.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(GetMethodData)
                .ToList();

            return result;
        }

        private static MethodData GetMethodData(MethodDeclarationSyntax method)
        {
            var result = new MethodData
            {
                Name = method.Identifier.Text,
                ReturnType = method.ReturnType.ToTypeData().ToTypeString(),
            };

            // Resolve args list
            var arguments = method.ParameterList.Parameters
                .Select(item => (name: item.Identifier.Text, type: item.Type!.ToTypeData()))
                .ToList();
            result.Parameters = arguments.Select(item => item.name).ToList();
            result.ParameterTypes = arguments.Select(item => item.type.ToTypeString()).ToList();

            // Resolve generic args list
            string[]? genericTypes = null;
            if (method.TypeParameterList is not null)
            {
                var typeParameters = method.TypeParameterList.Parameters;
                if (typeParameters.Any())
                {
                    result.TypeParameters = typeParameters.Select(item => item.Identifier.ValueText).ToArray();
                }
            }

            // Resolve return type
            result.IsVoid = result.ReturnType is "void";
            if (result.ReturnType is "Task")
            {
                result.IsVoid = true;
                result.IsAwaitable = true;
            }

            if (method.ReturnType is GenericNameSyntax { Identifier: { ValueText: "Task" } })
            {
                result.IsAwaitable = true;
                result.TaskReturnType = method.ReturnType.ToTypeData().Children!.Single().ToTypeString();
                result.TaskReturnTypeContainsGenericParameter =
                    genericTypes is not null && method.ReturnType.ToTypeData().ContainsTypes(genericTypes);
            }

            return result;
        }

        private static IEnumerable<InterfaceDeclarationSyntax> SelectIpcContractInterfaces(IEnumerable<InterfaceDeclarationSyntax> source)
        {
            return source
                .Where(@interface => @interface.AttributeLists
                    .Any(attributeList => attributeList.Attributes
                        .Any(attribute =>
                        {
                            var attributeName = attribute.Name.ToFullString();
                            return attributeName is "IpcContract" or "HandyIpc.IpcContract";
                        })))
                .Where(@interface => @interface.Members.OfType<MethodDeclarationSyntax>().Any());
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<InterfaceDeclarationSyntax> CandidateInterfaces { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax &&
                    interfaceDeclarationSyntax.AttributeLists.Count > 0)
                {
                    CandidateInterfaces.Add(interfaceDeclarationSyntax);
                }
            }
        }
    }
}
