using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static HandyIpc.Generator.TemplateExtensions;

namespace HandyIpc.Generator
{
    public static class Dispatcher
    {
        public static string Generate(INamedTypeSymbol @interface, IReadOnlyCollection<IMethodSymbol> methods)
        {
            var (@namespace, className, typeParameters) = @interface.GenerateNameFromInterface();
            string interfaceType = @interface.ToFullDeclaration();

            return $@"
namespace {@namespace}
{{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using HandyIpc.Core;

    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Reflection.Obfuscation(Exclude = true)]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public class {nameof(Dispatcher)}{className}{typeParameters} : IIpcDispatcher
{@interface.TypeParameters.For(typeParameter => $@"
        {typeParameter.ToGenericConstraint()}
")}
    {{
        private readonly {interfaceType} _instance;
{Text(methods.Any(item => item.TypeParameters.Any()) ? @"
        private readonly Lazy<IReadOnlyDictionary<string, MethodInfo>> _genericMethodMapping;
" : RemoveLineIfEmpty)}

        public {nameof(Dispatcher)}{className}({interfaceType} instance)
        {{
            _instance = instance;
{Text(methods.Any(item => item.TypeParameters.Any()) ? $@"
            _genericMethodMapping = new Lazy<IReadOnlyDictionary<string, MethodInfo>>(
                () => GeneratorHelper.GetGenericMethodMapping(typeof({interfaceType}), _instance));
" : RemoveLineIfEmpty)}
        }}

        public async Task Dispatch(Context ctx, Func<Task> next)
        {{
            var request = ctx.Request;
            if (request is null)
            {{
                throw new InvalidOperationException($""The {{nameof(Context.Request)}} must be parsed from {{nameof(Context.Input)}} before it can be used."");
            }}

            switch (request.MethodName)
            {{
{methods.For(method =>
{
    string methodId = method.GenerateMethodId();
    string arguments = method.Parameters
        .Select(item => item.Type)
        .Select(item => item.ToTypeDeclaration())
        .Select((type, i) => $"({type})args[{i}]").Join(", ");
    bool isAwaitable = method.ReturnType.IsAwaitable();
    bool isVoid = method.ReturnType.IsVoid();
    bool containsTypeParameter = method.ReturnType is INamedTypeSymbol namedTypeSymbol &&
                                 EnumerateTypeTree(namedTypeSymbol)
                                     .Any(returnType => method
                                         .TypeArguments
                                         .Any(methodType => methodType
                                             .Equals(returnType, SymbolEqualityComparer.Default)));

    return $@"
{Text(method.TypeParameters.Any() ? $@"
                case ""{methodId}""
                when (_genericMethodMapping.Value.TryGetValue(""{methodId}"", out var methodInfo)):
                {{
{Text(method.Parameters.Any() ? @"
                    var args = request.Arguments.ToArray();
" : RemoveLineIfEmpty)}
                    var constructedMethodInfo = methodInfo.MakeGenericMethod(request.MethodTypeArguments.ToArray());
                    var obj = constructedMethodInfo.Invoke(_instance, {(method.Parameters.Any() ? "args" : "new object[0]")});
{Text(isVoid ? $@"
                    {(isAwaitable ? "await (Task)obj;" : null)}
                    ctx.Output = Signals.Unit;
" : $@"
{Text(containsTypeParameter ? @"
                    var result = await GeneratorHelper.UnpackTask(constructedMethodInfo.ReturnType, obj);
                    ctx.Output = ctx.Serializer.SerializeResponseValue(result, constructedMethodInfo.ReturnType);
" : $@"
                    var result = {(isAwaitable ? $"await ({method.ReturnType.ToTypeDeclaration()})" : null)}obj;
                    ctx.Output = ctx.Serializer.SerializeResponseValue(result, constructedMethodInfo.ReturnType);
")}
")}
                    break;
                }}
" : $@"
                case ""{methodId}"":
                {{
{method.Parameters
    .Select(item => item.Type)
    .Select(item => item.ToFullDeclaration())
    .Select(type => $"typeof({type})").Join(", ").If(text => $@"
                    request.ArgumentTypes = new Type[] {{ {text} }};
                    var args = request.Arguments;
", RemoveLineIfEmpty)}
{Text(isVoid ? $@"
                    {(isAwaitable ? "await " : null)}_instance.{method.Name}({arguments});
                    ctx.Output = Signals.Unit;
" : $@"
                    var result = {(isAwaitable ? "await " : null)}_instance.{method.Name}({arguments});
                    ctx.Output = ctx.Serializer.SerializeResponseValue(result, typeof({method.ReturnType.ToFullDeclaration()}));
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
".FormatCode();
        }

        public static IEnumerable<ITypeSymbol> EnumerateTypeTree(INamedTypeSymbol type)
        {
            yield return type;
            foreach (ITypeSymbol typeSymbol in type.TypeArguments)
            {
                if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                {
                    continue;
                }

                foreach (ITypeSymbol child in EnumerateTypeTree(namedTypeSymbol))
                {
                    yield return child;
                }
            }
        }
    }
}
