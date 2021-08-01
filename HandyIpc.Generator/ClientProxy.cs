using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static HandyIpc.Generator.TemplateExtensions;

namespace HandyIpc.Generator
{
    public static class ClientProxy
    {
        public static string Generate(INamedTypeSymbol @interface, IReadOnlyCollection<IMethodSymbol> methods)
        {
            var (@namespace, className, typeParameters) = @interface.GenerateNameFromInterface();
            string interfaceType = @interface.ToFullDeclaration();

            return $@"
namespace {@namespace}
{{
    using System;
    using HandyIpc.Core;

    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Reflection.Obfuscation(Exclude = true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public class {nameof(ClientProxy)}{className}{typeParameters} : {interfaceType}
{@interface.TypeParameters.For(typeParameter => $@"
        {typeParameter.ToGenericConstraint()}
")}
    {{
        private readonly IRmiClient _client;
        private readonly string _identifier;
        private readonly string _accessToken;

        public {nameof(ClientProxy)}{className}(IRmiClient client, string identifier, string accessToken)
        {{
            _client = client;
            _identifier = identifier;
            _accessToken = accessToken;
        }}
{methods.For(method =>
{
    string methodName = $"{method.Name}{method.TypeParameters.Join(", ").If(text => $"<{text}>")}";
    var parameterTypes = method.Parameters
        .Select(item => item.Type)
        .Select(item => item.ToFullDeclaration())
        .ToList()
        .AsReadOnly();
    string methodId = method.GenerateMethodId();
    string parameters = method.Parameters.Select(parameter => $"{parameter.Type.ToFullDeclaration()} @{parameter.Name}").Join(", ");
    string arguments = method.Parameters.Select(parameter => $"new Argument(typeof({parameter.Type.ToFullDeclaration()}), @{parameter.Name})").Join(", ");
    bool isAwaitable = method.ReturnType.IsAwaitable();
    bool isVoid = method.ReturnType.IsVoid();

    return $@"

        /// <inheritdoc />
        {(isAwaitable ? "async " : null)}{method.ReturnType} {interfaceType}.{methodName}({parameters})
        {{
{Text(isAwaitable ? $@"
            var response = await _client.InvokeAsync<{(isVoid ? "byte[]" : ExtractTypeFromTask(method.ReturnType))}>(
" : $@"
            var response = _client.Invoke<{(isVoid ? "byte[]" : method.ReturnType.ToFullDeclaration())}>("
)}
                _identifier,
                new RequestHeader(""{methodId}"")
                {{
                    AccessToken = _accessToken,
                    {@interface.TypeParameters.Select(type => $"typeof({type.ToFullDeclaration()})").Join(", ").If(text => $"GenericArguments = new[] {{ {text} }},")}
                    {method.TypeParameters.Select(type => $"typeof({type.ToFullDeclaration()})").Join(", ").If(text => $"MethodGenericArguments = new[] {{ {text} }},")}
                    {(method.TypeParameters.Any() ? parameterTypes.Select(type => $"typeof({type})").Join(", ").If(text => $"ArgumentTypes = new[] {{ {text} }},") : null)}
                }},
                new Argument[] {{ {arguments} }});
{Text(isVoid ? @"
            if (!response.IsUnit())
            {
                throw new InvalidOperationException(""A method that returns void type must responses the Unit object."");
            }
" : @"
            return response;
")}
        }}
";
})}
    }}
}}
".FormatCode();
        }

        private static string ExtractTypeFromTask(ITypeSymbol symbol)
        {
            INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)symbol;
            return namedTypeSymbol.TypeArguments.Single().ToFullDeclaration();
        }
    }
}
