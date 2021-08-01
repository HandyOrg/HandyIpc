using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static HandyIpc.Generator.TemplateExtensions;

namespace HandyIpc.Generator
{
    public static class ServerProxy
    {
        public static string Generate(INamedTypeSymbol @interface, IReadOnlyCollection<IMethodSymbol> methods)
        {
            var (@namespace, className, typeParameters) = @interface.GenerateNameFromInterface();
            string interfaceType = @interface.ToFullDeclaration();

            return $@"
namespace {@namespace}
{{
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Reflection.Obfuscation(Exclude = true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public class {nameof(ServerProxy)}{className}{typeParameters} : {interfaceType}
{@interface.TypeParameters.For(typeParameter => $@"
        {typeParameter.ToGenericConstraint()}
")}
    {{
        private readonly {interfaceType} _instance;

        public {nameof(ServerProxy)}{className}({interfaceType} instance)
        {{
            _instance = instance;
        }}
{methods.For(method =>
{
    string methodName = method.ToFullDeclaration();
    string methodParameterList = method.Parameters
        .Select(parameter =>
        {
            bool nullable = !parameter.Type.IsValueType && parameter.NullableAnnotation == NullableAnnotation.Annotated;
            return $"{parameter.Type.ToFullDeclaration()}{(nullable ? "?" : string.Empty)} @{parameter.Name}";
        })
        .Join(", ");
    bool isAwaitable = method.ReturnType.IsAwaitable();
    bool isVoid = method.ReturnType.IsVoid();

    return $@"

        /// <inheritdoc />
{Text(method.TypeParameters.Any() ? $@"
        [global::HandyIpc.Core.IpcMethod(""{method.GenerateMethodId()}"")]
" : RemoveLineIfEmpty)}
        {method.ReturnType.ToFullDeclaration()} {interfaceType}.{methodName}({methodParameterList})
        {{
            {(!isVoid || isAwaitable ? "return " : null)}_instance.{methodName}({method.Parameters.Select(parameter => $"@{parameter.Name}").Join(", ")});
        }}
";
})}
    }}
}}
".FormatCode();
        }
    }
}
