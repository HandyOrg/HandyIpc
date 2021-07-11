﻿using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HandyIpc;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TypeExtensions
    {
        private const string AutoGeneratedPrefix = "HandyIpc";

        public static Type GetClientType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "Client");
        }

        public static Type GetDispatcherType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "Dispatcher");
        }

        public static Type GetServerProxyType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "ServerProxy");
        }

        private static Type GetAutoGeneratedType(Type interfaceType, string category)
        {
            string typeName;
            var prefix = AutoGeneratedPrefix + category;

            if (interfaceType.IsNested)
            {
                var className = prefix + interfaceType.DeclaringType!.Name + interfaceType.Name;
                typeName = interfaceType.AssemblyQualifiedName!.Replace(
                    interfaceType.DeclaringType.FullName + "+" + interfaceType.Name,
                    interfaceType.Namespace + "." + className);
            }
            else
            {
                var className = prefix + interfaceType.Name;

                if (interfaceType.Namespace == null)
                {
                    className = $"{className}.{className}";
                }

                typeName = interfaceType.AssemblyQualifiedName!.Replace(interfaceType.Name, className);
            }
            return Type.GetType(typeName) ??
                   throw new InvalidOperationException($"{interfaceType.Name} doesn't look like a Ipc interface. ");
        }

        public static string ResolveIdentifier(this Type interfaceType)
        {
            Guards.ThrowIfNot(interfaceType.IsInterface, "The type must be interface type.", nameof(interfaceType));

            var attribute = interfaceType.GetCustomAttribute<IpcContractAttribute>(false);
            string identifier = attribute.Identifier;

            if (string.IsNullOrEmpty(identifier))
            {
                using var sha256 = new SHA256CryptoServiceProvider();
                var buffer = Encoding.UTF8.GetBytes(interfaceType.AssemblyQualifiedName!);
                var sha256Bytes = sha256.ComputeHash(buffer);
                identifier = string.Concat(sha256Bytes.Select(item => item.ToString("X2")));
            }

            return identifier;
        }
    }
}

namespace HandyIpc.Server
{
    using System;

    public static class TypeExtensions
    {
        /// <summary>
        /// Unpack the result of the <see cref="Task"/> or <see cref="Task{T}"/> with the specified instance and the related type.
        /// </summary>
        /// <remarks>
        /// It is used to get the result from a invoked async generic method on server side.
        /// </remarks>
        /// <param name="taskType">The the <see cref="Task"/> or <see cref="Task{T}"/> type.</param>
        /// <param name="object">The instance of the task type.</param>
        /// <returns>The result from the task instance.</returns>
        public static async Task<object?> UnpackTask(this Type taskType, object @object)
        {
            if (@object is not Task task) return null;

            await task;
            return taskType.GetProperty("Result")?.GetMethod.Invoke(@object, null);
        }
    }
}
