using System.Linq;
using HandyIpc.Generator.Data;

namespace HandyIpc.Generator
{
    public static class ServerProxy
    {
        public static string Generate(TemplateFileData data)
        {
            return $@"
using HandyIpc.Core;
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
    public class ServerProxy{@class.GeneratedClassSuffix}{interfaceTypeParameters} : {interfaceType}
        {@class.ConstraintClauses}
    {{
        private readonly {interfaceType} _instance;

        public ServerProxy{@class.GeneratedClassSuffix}({interfaceType} instance) 
        {{
            _instance = instance;
        }}
{@class.MethodList.For(method =>
            {
                string methodName = $"{method.Name}{method.TypeParameters.Join(", ").If(text => $"<{text}>")}";
                string methodParameterList = method.ParameterTypes.Zip(method.Parameters, (type, parameter) => $"{type} {parameter}").Join(", ");
                return $@"

        [IpcMethod(""{methodName}({method.ParameterTypes.Join(", ")})"")]
        {method.ReturnType} {interfaceType}.{methodName}({methodParameterList})
        {{
            {(!method.IsVoid || method.IsAwaitable).If("return ")}{$"_instance.{methodName}({method.Parameters.Join(", ")});"}
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
