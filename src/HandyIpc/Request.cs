using System;
using System.Reflection;

namespace HandyIpc
{
    /// <summary>
    /// Represents a request for a call to a remote method.
    /// </summary>
    [Obfuscation(Exclude = true)]
    public class Request
    {
        /// <summary>
        /// Gets the access token, which may be empty/null.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName { get; set; } = null!;

        /// <summary>
        /// Gets the arguments on the method, which may be null.
        /// </summary>
        public object?[]? Arguments { get; set; }

        /// <summary>
        /// Gets types of arguments on the method, which may be null.
        /// </summary>
        /// <remarks>
        /// The property has been filled only if the method is a generic method.
        /// </remarks>
        public Type[]? ArgumentTypes { get; set; }

        /// <summary>
        /// Gets the generic argument that are defined on the interface, which may be null.
        /// </summary>
        public Type[]? GenericArguments { get; set; }

        /// <summary>
        /// Gets the generic arguments that are defined on the method, which may be null.
        /// </summary>
        public Type[]? MethodGenericArguments { get; set; }
    }
}
