using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace HandyIpc.Generator
{
    internal static class Extensions
    {
        public static INamedTypeSymbol TaskTypeSymbol { get; set; } = null!;

        public static string ToFullDeclaration(this ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public static string ToTypeDeclaration(this ITypeSymbol symbol)
        {
            bool nullable = !symbol.IsValueType && symbol.NullableAnnotation == NullableAnnotation.Annotated;
            return $"{symbol.ToFullDeclaration()}{(nullable ? '?' : null)}";
        }

        public static string ExtractTypeFromTask(this ITypeSymbol symbol)
        {
            if (symbol.IsOrInheritsFrom(TaskTypeSymbol))
            {
                INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)symbol;
                symbol = namedTypeSymbol.TypeArguments.Single();
            }

            return symbol.ToFullDeclaration();
        }

        public static (string @namespace, string className, string typeParameters) GenerateNameFromInterface(this INamedTypeSymbol interfaceSymbol)
        {
            const string globalNamespace = "HandyIpc.Implementation";

            string classNameWithGeneric = interfaceSymbol.ToDisplayString();
            int lastDot = classNameWithGeneric.LastIndexOf('.');
            if (lastDot > 0)
            {
                classNameWithGeneric = classNameWithGeneric.Substring(lastDot + 1);
            }

            string className = interfaceSymbol.Name;
            string @namespace = interfaceSymbol.ContainingNamespace?.ToDisplayString() ?? globalNamespace;
            if (interfaceSymbol.ContainingNamespace is { IsGlobalNamespace: true })
            {
                @namespace = globalNamespace;
            }

            string containingType = interfaceSymbol.ContainingType is not null ? $"{interfaceSymbol.ContainingType.Name}_" : string.Empty;

            return (
                @namespace,
                className: $"{containingType}{className}",
                typeParameters: classNameWithGeneric.Substring(className.Length));
        }

        public static string ToGenericConstraint(this ITypeParameterSymbol typeParameter)
        {
            var parameters = new List<string>();
            if (typeParameter.HasReferenceTypeConstraint)
            {
                parameters.Add("class");
            }

            if (typeParameter.HasUnmanagedTypeConstraint)
            {
                parameters.Add("unmanaged");
            }

            if (typeParameter.HasValueTypeConstraint)
            {
                parameters.Add("struct");
            }

            if (typeParameter.HasNotNullConstraint)
            {
                parameters.Add("notnull");
            }

            parameters.AddRange(typeParameter.ConstraintTypes.Select(ToFullDeclaration));

            if (typeParameter.HasConstructorConstraint)
            {
                parameters.Add("new()");
            }

            return parameters.Count > 0
                ? $"where { typeParameter.Name} : { string.Join(", ", parameters)}"
                : string.Empty;
        }

        public static bool IsOrInheritsFrom(this ITypeSymbol self, ITypeSymbol other)
        {
            return self.EnumerateSelfAndBaseType().Any(item => item.Equals(other, SymbolEqualityComparer.Default));
        }

        public static bool ReturnsVoidTask(this ITypeSymbol symbol)
        {
            return symbol.Equals(TaskTypeSymbol, SymbolEqualityComparer.Default);
        }

        public static bool IsAwaitable(this ITypeSymbol type)
        {
            return type.IsOrInheritsFrom(TaskTypeSymbol);
        }

        public static string GenerateMethodId(this IMethodSymbol method)
        {
            string methodName = $"{method.Name}{method.TypeParameters.Join(", ").If(text => $"<{text}>")}";

            return $"{methodName}({method.Parameters.Select(item => item.Type).Select(item => item.ToFullDeclaration()).Join(", ")})";
        }

        public static IEnumerable<ITypeSymbol> EnumerateSelfAndBaseType(this ITypeSymbol symbol)
        {
            ITypeSymbol? current = symbol;
            while (current is not null)
            {
                yield return current;
                current = current.BaseType;
            }
        }
    }
}
