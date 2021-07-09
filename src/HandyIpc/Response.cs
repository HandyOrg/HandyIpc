using System;
using System.Reflection;

namespace HandyIpc
{
    /// <summary>
    /// Represents the result returned by a remote method.
    /// </summary>
    /// <remarks>
    /// Similar to the Result enum in Rust lang, but C# does not support such syntax.
    /// </remarks>
    [Obfuscation(Exclude = true)]
    public class Response
    {
        /// <summary>
        /// Gets or sets a value that represents the result returned by the function.
        /// If the <see cref="Exception"/> is null, it is valid; otherwise, it should not be used.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets a exception instance, if an exception occurred on this call.
        /// </summary>
        public Exception? Exception { get; set; }
    }
}
