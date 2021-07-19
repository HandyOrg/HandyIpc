using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HandyIpc.Generator
{
    public static class TemplateExtensions
    {
        public static string Line<T>(this IEnumerable<T> self, Func<T, string> callback)
        {
            return self
                .Aggregate(
                    new StringBuilder(),
                    (sb, item) => sb.AppendLine(callback(item).TrimNewLine()))
                .ToString();
        }

        public static string Join<T>(this IEnumerable<T> self, string separator, Func<T, string>? callback = null)
        {
            return string.Join(separator, self.Select(item => callback?.Invoke(item) ?? item?.ToString() ?? string.Empty));
        }

        public static string If(this string? self, Func<string, string> ifTrueCallback, string? ifFalseText = null)
        {
            return string.IsNullOrWhiteSpace(self) ? (ifFalseText?.TrimNewLine() ?? string.Empty) : ifTrueCallback(self!.Trim('\r', '\n'));
        }

        public static string If(this bool self, string ifTrueText, string? ifFalseText = null)
        {
            return self ? ifTrueText.TrimNewLine() : ifFalseText?.TrimNewLine() ?? string.Empty;
        }

        private static string TrimNewLine(this string? text) => text?.Trim('\r', '\n') ?? string.Empty;
    }
}
