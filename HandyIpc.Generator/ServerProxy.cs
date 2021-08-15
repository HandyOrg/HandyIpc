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
    var parameterNames = method.Parameters.Select(parameter => $"{parameter.Name}_").ToList();
    string parameters = method.Parameters
        .Select(item => item.Type.ToTypeDeclaration())
        .Zip(parameterNames, (type, parameter) => $"{type} {parameter}")
        .Join(", ");
    bool isAwaitable = method.ReturnType.IsAwaitable();
    bool isVoid = method.ReturnsVoid;

    return $@"

        /// <inheritdoc />
{Text(method.TypeParameters.Any() ? $@"
        [global::HandyIpc.Core.IpcMethod(""{method.GenerateMethodId()}"")]
" : RemoveLineIfEmpty)}
        {method.ReturnType.ToTypeDeclaration()} {interfaceType}.{methodName}({parameters})
        {{
            {(!isVoid || isAwaitable ? "return " : null)}_instance.{methodName}({parameterNames.Join(", ")});
        }}
";
})}
    }}
}}
".FormatCode();
        }
    }
}
