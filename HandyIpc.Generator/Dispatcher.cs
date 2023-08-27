using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static HandyIpc.Generator.TemplateExtensions;

namespace HandyIpc.Generator
{
    public static class Dispatcher
    {
        public static string Generate(INamedTypeSymbol @interface, IReadOnlyCollection<IMethodSymbol> methods, IReadOnlyCollection<IEventSymbol> events)
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
    public class {nameof(Dispatcher)}{className}{typeParameters} : IMethodDispatcher{(events.Any() ? ", INotifiable" : null)}
{@interface.TypeParameters.For(typeParameter => $@"
        {typeParameter.ToGenericConstraint()}
")}
    {{
        private readonly {interfaceType} _instance;
{Text(methods.Any(item => item.TypeParameters.Any()) ? @"
        private readonly Lazy<IReadOnlyDictionary<string, MethodInfo>> _genericMethodMapping;
" : RemoveLineIfEmpty)}

{Text(events.Any() ? @"
        public NotifierManager NotifierManager { get; set; }

" : RemoveLineIfEmpty)}
        public {nameof(Dispatcher)}{className}({interfaceType} instance)
        {{
            _instance = instance;
{Text(methods.Any(item => item.TypeParameters.Any()) ? $@"
            _genericMethodMapping = new Lazy<IReadOnlyDictionary<string, MethodInfo>>(
                () => GeneratorHelper.GetGenericMethodMapping(typeof({interfaceType}), _instance));
" : RemoveLineIfEmpty)}
{events.For(item => $@"
            instance.{item.Name} += (_, e) => NotifierManager.Publish(""{item.Name}"", e);
")}
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
    bool isVoid = method.ReturnsVoid || method.ReturnType.ReturnsVoidTask();
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
                    var constructedMethodInfo = methodInfo.MakeGenericMethod(request.MethodTypeArguments.ToArray());
{Text(method.Parameters.Any() ? @"
                    request.SetArgumentTypes(constructedMethodInfo.GetParameters().Select(item => item.ParameterType).ToArray());
                    var args = request.Arguments.ToArray();
" : RemoveLineIfEmpty)}
                    var obj = constructedMethodInfo.Invoke(_instance, {(method.Parameters.Any() ? "args" : "new object[0]")});
{Text(isVoid ? $@"
                    {(isAwaitable ? "await (Task)obj;" : null)}
                    ctx.Output = Response.Unit;
" : $@"
{Text(containsTypeParameter && isAwaitable ? @"
                    var result = await GeneratorHelper.UnpackTask(constructedMethodInfo.ReturnType, obj);
" : $@"
                    var result = {(isAwaitable ? $"await ({method.ReturnType.ToTypeDeclaration()})" : null)}obj;
")}
                    ctx.Output = Response.Value(result, GeneratorHelper.ExtractTaskValueType(constructedMethodInfo.ReturnType), ctx.Serializer);
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
                    request.SetArgumentTypes(new Type[] {{ {text} }});
                    var args = request.Arguments;
", RemoveLineIfEmpty)}
{Text(isVoid ? $@"
                    {(isAwaitable ? "await " : null)}_instance.{method.Name}({arguments});
                    ctx.Output = Response.Unit;
" : $@"
                    var result = {(isAwaitable ? "await " : null)}_instance.{method.Name}({arguments});
                    ctx.Output = Response.Value(result, typeof({method.ReturnType.ExtractTypeFromTask()}), ctx.Serializer);
")}
                    break;
                }}
")}
";
})}
                default:
                    throw new ArgumentOutOfRangeException(""No matching remote method was found."");
            }}
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
                    yield return typeSymbol;
                    yield break;
                }

                foreach (ITypeSymbol child in EnumerateTypeTree(namedTypeSymbol))
                {
                    yield return child;
                }
            }
        }
    }
}
