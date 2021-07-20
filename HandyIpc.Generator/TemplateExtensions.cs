using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HandyIpc.Generator
{
    public static class TemplateExtensions
    {
        private static readonly int NewLineLength = Environment.NewLine.Length;
        private static readonly Regex WhiteSpaceLineRegex = new($@"{Environment.NewLine}( +?{Environment.NewLine})+", RegexOptions.Compiled);

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
            return string.IsNullOrEmpty(self)
                ? ifFalseText?.TrimNewLine() ?? string.Empty
                : ifTrueCallback(self!.TrimNewLine()).TrimNewLine();
        }

        public static string IfLine(this string? self, Func<string, string> ifTrueCallback)
        {
            return self.If(ifTrueCallback, " ");
        }

        public static string If(this bool self, string ifTrueText, string? ifFalseText = null)
        {
            return self ? ifTrueText.TrimNewLine() : ifFalseText?.TrimNewLine() ?? string.Empty;
        }

        public static string IfLine(this bool self, string text)
        {
            return self.If(text, " ");
        }

        public static string RemoveWhiteSpaceLine(this string text)
        {
            return WhiteSpaceLineRegex.Replace(text, Environment.NewLine);
        }

        private static string TrimNewLine(this string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            int start = text!.StartsWith(Environment.NewLine) ? NewLineLength : 0;
            int length = text.EndsWith(Environment.NewLine)
                ? text.Length - start - NewLineLength
                : text.Length - start;

            return text.Substring(start, length);
        }
    }
}
