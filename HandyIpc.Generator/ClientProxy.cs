using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static HandyIpc.Generator.TemplateExtensions;

namespace HandyIpc.Generator
{
    public static class ClientProxy
    {
        public static string Generate(INamedTypeSymbol @interface, IReadOnlyCollection<IMethodSymbol> methods, IReadOnlyCollection<IEventSymbol> events)
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
{Text(events.Any() ? @"
        private readonly AwaiterManager _awaiterManager;
" : RemoveLineIfEmpty)}

{events.For(item => $@"
        private event {item.Type.ToTypeDeclaration()} _{item.Name};
")}
{events.For(item =>
            {
                IParameterSymbol eSymbol = ((INamedTypeSymbol)item.Type).DelegateInvokeMethod!.Parameters[1];
                string eType = eSymbol.Type.ToTypeDeclaration();
                return $@"

        public event {item.Type.ToTypeDeclaration()} {item.Name}
        {{
            add
            {{
                if (_{item.Name} == null)
                {{
                    _awaiterManager.Subscribe(""{item.Name}"", args =>
                    {{
                        var e = ({eType})_serializer.Deserialize(args, typeof({eType}));
                        _{item.Name}?.Invoke(this, e);
                    }});
                }}

                _{item.Name} += value;
            }}
            remove
            {{
                _{item.Name} -= value;
                if (_{item.Name} == null)
                {{
                    _awaiterManager.Unsubscribe(""{item.Name}"");
                }}
            }}
        }}
";
            })}

        public {nameof(ClientProxy)}{className}(Sender sender, ISerializer serializer, string key)
        {{
            _sender = sender;
            _serializer = serializer;
            _key = key;
{Text(events.Any() ? @"
            _awaiterManager = new AwaiterManager(key, sender, serializer);
" : RemoveLineIfEmpty)}
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
