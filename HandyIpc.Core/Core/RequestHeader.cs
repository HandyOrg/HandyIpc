using System;
using System.Reflection;

namespace HandyIpc.Core
{
    /// <summary>
    /// Represents a request for a call to a remote method.
    /// </summary>
    [Obfuscation(Exclude = true)]
    public class RequestHeader
    {
        /// <summary>
        /// Gets the access token, which may be empty/null.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets the generic argument that are defined on the interface, which may be null.
        /// </summary>
        public Type[]? GenericArguments { get; set; }

        /// <summary>
        /// Gets the generic arguments that are defined on the method, which may be null.
        /// </summary>
        public Type[]? MethodGenericArguments { get; set; }

        /// <summary>
        /// Gets types of arguments on the method, which may be null.
        /// </summary>
        /// <remarks>
        /// The property has been filled only if the method is a generic method.
        /// </remarks>
        public Type[]? ArgumentTypes { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="RequestHeader"/> type with the specified method name.
        /// </summary>
        /// <param name="methodName">The specified method name.</param>
        public RequestHeader(string methodName) => MethodName = methodName;
    }
}
