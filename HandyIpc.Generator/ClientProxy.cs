using System.Linq;
using HandyIpc.Generator.Data;

namespace HandyIpc.Generator
{
    public static class ClientProxy
    {
        public static string Generate(TemplateFileData data)
        {
            return $@"
using System;
using HandyIpc;
using HandyIpc.Client;
{data.ClassList.For(@class =>
            {
                string interfaceTypeParameters = @class.TypeParameters.Join(", ").If(text => $"<{text}>");
                string interfaceType = $"{@class.InterfaceName}{interfaceTypeParameters}";
                return $@"

namespace {@class.Namespace}
{{
{data.UsingList.For(@using => $@"
    using {@using};
")}
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Reflection.Obfuscation(Exclude = true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public class ClientProxy{@class.GeneratedClassSuffix}{interfaceTypeParameters} : {interfaceType}
        {@class.ConstraintClauses}
    {{
        private readonly IRmiClient _client;
        private readonly string _identifier;
        private readonly string _accessToken;

        public ClientProxy{@class.GeneratedClassSuffix}(IRmiClient client, string identifier, string accessToken)
        {{
            _client = client;
            _identifier = identifier;
            _accessToken = accessToken;
        }}
{@class.MethodList.For(method =>
                {
                    string methodName = $"{method.Name}{method.TypeParameters.Join(", ").If(text => $"<{text}>")}";
                    string methodId = $"{methodName}({method.ParameterTypes.Join(", ")})";
                    string methodParameterList = method.ParameterTypes.Zip(method.Parameters, (type, parameter) => $"{type} {parameter}").Join(", ");
                    return $@"

        {method.IsAwaitable.If("async ")}{method.ReturnType} {interfaceType}.{methodName}({methodParameterList})
        {{
{method.IsAwaitable.If($@"
            var response = await _client.InvokeAsync<{method.IsVoid.If("byte[]", method.TaskReturnType)}>(",
                        $@"
            var response = _client.Invoke<{method.IsVoid.If("byte[]", method.ReturnType)}>("
                        )}
                _identifier,
                new RequestHeader(""{methodId}"")
                {{
                    AccessToken = _accessToken,
                    {@class.TypeParameters.Select(type => $"typeof({type})").Join(", ").If(text => $"GenericArguments = new[] {{ {text} }},")}
                    {method.TypeParameters.Select(type => $"typeof({type})").Join(", ").If(text => $"MethodGenericArguments = new[] {{ {text} }},")}
                    {method.TypeParameters.Any().If(method.ParameterTypes.Select(type => $"typeof({type})").Join(", ").If(text => $"ArgumentTypes = new[] {{ {text} }},"))}
                }},
                Signals.GetArgumentList(
                    {method.Parameters.Join(", ").If(text => $"new object[] {{ {text} }},", "null,")}
                    {method.ParameterTypes.Select(type => $"typeof({type})").Join(", ").If(text => $"new Type[] {{ {text} }}", "null")}
                ));
{method.IsVoid.If(@"
            if (!response.IsUnit())
            {
                throw new InvalidOperationException(""A method that returns void type must responses the Unit object."");
            }", @"
            return response;")}
        }}
";
                })}
    }}
}}
";
            })}
".RemoveWhiteSpaceLine();
        }
    }
}
