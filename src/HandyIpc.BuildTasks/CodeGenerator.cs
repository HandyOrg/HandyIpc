using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandyIpc.BuildTasks.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HandyIpc.BuildTasks
{
    public static class CodeGenerator
    {
        public static TemplateFileData GetTemplateData(IEnumerable<string> filePaths)
        {
            var validInterfaces = filePaths
                .Select(x => CSharpSyntaxTree.ParseText(File.ReadAllText(x)))
                .SelectMany(FindValidInterfaces)
                .OrderBy(i => i.Identifier.Text)
                .ToList();

            var usingList = validInterfaces
                .SelectMany(@interface => @interface
                    .GetSyntaxNodeRoot<SyntaxNode>()
                    .DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(x => $"{x.Alias} {x.StaticKeyword} {x.Name}".TrimStart()))
                .Distinct()
                .Where(item => item != "HandyIpc")
                .ToList();

            var classList = validInterfaces.Select(GetClassData).ToList();
            if (classList.Any(classData => classData.HasGenericMethod))
            {
                usingList.AddIfMissing("System.Collections.Generic");
                usingList.AddIfMissing("System.Reflection");
            }

            return new TemplateFileData(usingList, classList);
        }

        private static ClassData GetClassData(InterfaceDeclarationSyntax @interface)
        {
            var result = new ClassData();

            result.InterfaceName = @interface.GetInterfaceName();
            result.GeneratedClassSuffix = result.InterfaceName.Replace(".", string.Empty);
            result.Modifiers = @interface.Modifiers
                .Select(item => item.ValueText)
                .FirstOrDefault(item => item == "public" || item == "internal");
            var parent = @interface.GetSyntaxNodeRoot<NamespaceDeclarationSyntax>();
            result.Namespace = parent?.Name?.ToString() ?? $"HandyIpc{result.GeneratedClassSuffix}";

            if (@interface.TypeParameterList is not null)
            {
                var typeParameters = @interface.TypeParameterList.Parameters;
                if (typeParameters.Any())
                {
                    var types = typeParameters.Select(item => item.Identifier.ValueText).ToList();
                    result.TypeParameters = types.ToListString();
                    result.TypeArguments = types.Select(item => $"typeof({item})").ToListString();
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
            result.Parameters = arguments.Select(item => item.name).ToListString();
            result.ParameterTypes = arguments.Select(item => item.type.ToTypeString()).ToListString();
            result.TypeAndParameters = arguments
                .Select(item => $"{item.type.ToTypeString()} {item.name}")
                .ToListString();
            result.Arguments = arguments
                .Select((item, i) => $"({item.type.ToTypeString()})args[{i}]")
                .ToListString();
            // FIXME: If parameters do not contain generic parameters of the method,
            // parameter types will not need to be passed, it can be automatically generated in the server-side template.
            result.MethodParameterTypes = arguments.Select(item => $"typeof({item.type.ToTypeString()})").ToListString();

            // Resolve generic args list
            string[]? genericTypes = null;
            if (method.TypeParameterList is not null)
            {
                var typeParameters = method.TypeParameterList.Parameters;
                if (typeParameters.Any())
                {
                    genericTypes = typeParameters.Select(item => item.Identifier.ValueText).ToArray();
                    result.MethodTypeParameters = genericTypes.ToListString();
                    result.MethodTypeArguments = genericTypes.Select(item => $"typeof({item})").ToListString();
                }

                result.MethodConstraintClauses = method.ConstraintClauses.ToFullString().Trim();
            }

            // Resolve return type
            result.IsVoid = result.ReturnType == "void";
            if (result.ReturnType == "Task")
            {
                result.IsVoid = true;
                result.IsAwaitable = true;
            }

            if (method.ReturnType is GenericNameSyntax genericName &&
                genericName.Identifier.ValueText == "Task")
            {
                result.IsAwaitable = true;
                result.TaskReturnType = method.ReturnType.ToTypeData().Children!.Single().ToTypeString();
                result.TaskReturnTypeContainsGenericParameter =
                    genericTypes is not null && method.ReturnType.ToTypeData().ContainsTypes(genericTypes);
            }

            return result;
        }

        private static IEnumerable<InterfaceDeclarationSyntax> FindValidInterfaces(SyntaxTree tree)
        {
            var nodes = tree.GetRoot().DescendantNodes().ToList();

            return nodes.OfType<InterfaceDeclarationSyntax>()
                .Where(@interface => @interface.AttributeLists
                    .Any(attributeList => attributeList.Attributes
                        .Any(attribute =>
                        {
                            var attributeName = attribute.Name.ToFullString();
                            return attributeName == "IpcContract" || attributeName == "HandyIpc.IpcContract";
                        })))
                .Where(@interface => @interface.Members.OfType<MethodDeclarationSyntax>().Any());
        }
    }
}
