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
        private readonly Sender _sender;
        private readonly ISerializer _serializer;
        private readonly string _key;

        public {nameof(ClientProxy)}{className}(Sender sender, ISerializer serializer, string key)
        {{
            _sender = sender;
            _serializer = serializer;
            _key = key;
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
    // Add underscores to the end of parameters to avoid user fields with the same name as the generated local variables.
    var parameterNames = method.Parameters.Select(parameter => $"{parameter.Name}_").ToList();
    string parameters = method.Parameters
        .Select(item => item.Type.ToTypeDeclaration())
        .Zip(parameterNames, (type, parameter) => $"{type} {parameter}")
        .Join(", ");
    bool isAwaitable = method.ReturnType.IsAwaitable();
    bool isVoid = method.ReturnsVoid || method.ReturnType.ReturnsVoidTask();

    return $@"

        /// <inheritdoc />
        {(isAwaitable ? "async " : null)}{method.ReturnType.ToTypeDeclaration()} {interfaceType}.{methodName}({parameters})
        {{
{Text($@"
            var request = new Request(_serializer, ""{methodId}"")
            {{
                Name = _key,
{@interface.TypeParameters.Select(type => $"typeof({type.ToFullDeclaration()})").Join(", ").If(text => $@"
                TypeArguments = new[] {{ {text} }},
", RemoveLineIfEmpty)}
{method.TypeParameters.Select(type => $"typeof({type.ToFullDeclaration()})").Join(", ").If(text => $@"
                MethodTypeArguments = new[] {{ {text} }},
", RemoveLineIfEmpty)}
{parameterNames.Join(", ").If(text => $@"
                Arguments = new object[] {{ {text} }},
", RemoveLineIfEmpty)}
            }};
")}
{parameterTypes.Select(type => $"typeof({type})").Join(", ").If(text => $@"
            request.SetArgumentTypes(new[] {{ {text} }});
", RemoveLineIfEmpty)}
{Text(isAwaitable ? @"
            var responseBytes = await _sender.InvokeAsync(request.ToBytes());
" : @"
            var responseBytes = _sender.Invoke(request.ToBytes());
"
)}
{Text(isAwaitable ? $@"
            var response = GeneratorHelper.UnpackResponse<{(isVoid ? "byte[]" : method.ReturnType.ExtractTypeFromTask())}>(responseBytes, _serializer);
" : $@"
            var response = GeneratorHelper.UnpackResponse<{(isVoid ? "byte[]" : method.ReturnType.ToTypeDeclaration())}>(responseBytes, _serializer);
")}

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
    }
}
