﻿using System.Collections.Generic;
using System.Linq;
using HandyIpc.BuildTasks.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HandyIpc.BuildTasks
{
    internal static class Extensions
    {
        public static string GetInterfaceName(this InterfaceDeclarationSyntax @interface)
        {
            var identifier = @interface.Identifier;
            var interfaceParent = identifier.Parent is not null ? identifier.Parent.Parent : identifier.Parent;

            if ((interfaceParent as ClassDeclarationSyntax) is not null)
            {
                var classParent = ((ClassDeclarationSyntax)interfaceParent).Identifier;
                return classParent + "." + identifier.ValueText;
            }

            return identifier.ValueText;
        }

        public static T GetSyntaxNodeRoot<T>(this SyntaxNode node) where T : SyntaxNode
        {
            var root = node;
            while (root.Parent is T)
            {
                root = root.Parent;
            }

            return (T)root;
        }

        public static TypeData ToTypeData(this TypeSyntax typeSyntax)
        {
            return typeSyntax is GenericNameSyntax generic
                ? new TypeData(generic.Identifier.ValueText)
                {
                    Children = generic.TypeArgumentList.Arguments.Select(a => a.ToTypeData()).ToList()
                }
                : new TypeData(typeSyntax.ToString());
        }

        public static string ToTypeString(this TypeData typeData)
        {
            return typeData.Name + (typeData.Children?.Count > 0
                       ? $"<{typeData.Children.Select(item => item.ToTypeString()).ToListString()}>"
                       : string.Empty);
        }

        public static bool ContainsTypes(this TypeData typeData, params string[] types)
        {
            if (types.Contains(typeData.Name)) return true;
            if (typeData.Children is not null)
            {
                foreach (var typeDataChild in typeData.Children)
                {
                    if (typeDataChild.ContainsTypes(types))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string ToListString(this IEnumerable<string> items)
        {
            return string.Join(", ", items);
        }

        public static void AddIfMissing<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
    }
}
