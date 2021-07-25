﻿using System.Linq;
using HandyIpc.Generator.Data;

namespace HandyIpc.Generator
{
    public static class Dispatcher
    {
        public static string Generate(TemplateFileData data)
        {
            return $@"
using System;
using HandyIpc;
using HandyIpc.Server;
using HandyIpc.Implementation;
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
    public class Dispatcher{@class.GeneratedClassSuffix}{interfaceTypeParameters} : IIpcDispatcher
        {@class.ConstraintClauses}
    {{
        private readonly {interfaceType} _instance;
{@class.MethodList.Any(item => item.TypeParameters.Any()).IfLine(@"
        private readonly Lazy<IReadOnlyDictionary<string, MethodInfo>> _genericMethodMapping;
")}

        public Dispatcher{@class.GeneratedClassSuffix}({interfaceType} instance)
        {{
            _instance = instance;
{@class.MethodList.Any(item => item.TypeParameters.Any()).IfLine($@"
            _genericMethodMapping = new Lazy<IReadOnlyDictionary<string, MethodInfo>>(
                () => HandyIpcHelper.GetGenericMethodMapping(typeof({interfaceType}), _instance));
")}
        }}

        public async Task Dispatch(Context ctx, Func<Task> next)
        {{
            var request = ctx.RequestHeader;
            if (request is null)
            {{
                throw new InvalidOperationException($""The {{nameof(Context.RequestHeader)}} must be parsed from {{nameof(Context.Input)}} before it can be used."");
            }}

            switch (request.MethodName)
            {{
{@class.MethodList.For(method =>
                {
                    string methodName = $"{method.Name}{method.TypeParameters.Join(", ").If(text => $"<{text}>")}";
                    string methodId = $"{methodName}({method.ParameterTypes.Join(", ")})";
                    string arguments = method.ParameterTypes.Select((type, i) => $"({type})args[{i}]").Join(", ");
                    return $@"
{method.TypeParameters.Any().If($@"
                case ""{methodId}""
                when (_genericMethodMapping.Value.TryGetValue(""{methodId}"", out var methodInfo)):
                {{
{method.Parameters.Any().IfLine(@"
                    var args = ctx.Serializer.DeserializeRequestArguments(ctx.Input, request.ArgumentTypes);
")}
                    var constructedMethodInfo = methodInfo.MakeGenericMethod(request.MethodGenericArguments);
                    var obj = constructedMethodInfo.Invoke(_instance, {method.Parameters.Any().If("args", "new object[0]")});
{method.IsVoid.If($@"
                    {method.IsAwaitable.If("await (Task)obj;")}
                    ctx.Output = Signals.Unit;
", $@"
{method.TaskReturnTypeContainsGenericParameter.If(@"
                    var result = await HandyIpcHelper.UnpackTask(constructedMethodInfo.ReturnType, obj);
                    ctx.Output = ctx.Serializer.SerializeResponseValue(result, constructedMethodInfo.ReturnType);
", $@"
                    var result = {method.IsAwaitable.If($"await ({method.ReturnType})")}obj;
                    ctx.Output = ctx.Serializer.SerializeResponseValue(result, constructedMethodInfo.ReturnType);
")}
")}
                    break;
                }}
", $@"
                case ""{methodId}"":
                {{
{method.ParameterTypes.Select(type => $"typeof({type})").Join(", ").IfLine(text => $@"
                    var args = ctx.Serializer.DeserializeRequestArguments(ctx.Input, new Type[] {{ {text} }});
")}
{method.IsVoid.If($@"
                    {method.IsAwaitable.If("await ")}_instance.{method.Name}({arguments});
                    ctx.Output = Signals.Unit;
", $@"
                    var result = {method.IsAwaitable.If("await ")}_instance.{method.Name}({arguments});
                    ctx.Output = ctx.Serializer.SerializeResponseValue(result, typeof({method.ReturnType}));
")}
                    break;
                }}
")}
";
                })}
                default:
                    throw new ArgumentOutOfRangeException(""No matching remote method was found."");
            }}

            await next();
        }}
    }}
}}
";
            })}
".RemoveWhiteSpaceLine();
        }
    }
}
