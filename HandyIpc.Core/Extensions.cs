﻿using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HandyIpc.Server;

namespace HandyIpc
{
    public static class Extensions
    {
        public static IDisposable Start<TInterface, TImpl>(this IIpcServerHub server, string? accessToken = null)
            where TInterface : class
            where TImpl : TInterface, new()
        {
            return server.Start<TInterface>(() => new TImpl(), accessToken);
        }

        public static IDisposable Start<TInterface>(this IIpcServerHub server, Func<TInterface> factory, string? accessToken = null)
            where TInterface : class
        {
            return server.Start(typeof(TInterface), factory, accessToken);
        }

        public static IDisposable Start(this IIpcServerHub server, Type interfaceType, Type classType, string? accessToken = null)
        {
            // TODO: Add defensive code.

            return classType.ContainsGenericParameters
                ? server.Start(interfaceType, genericTypes =>
                {
                    var constructedClassType = classType.MakeGenericType(genericTypes);
                    return Activator.CreateInstance(constructedClassType);
                }, accessToken)
                : server.Start(interfaceType, () => Activator.CreateInstance(classType), accessToken);
        }

        internal static Type GetClientType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "ClientProxy");
        }

        internal static Type GetDispatcherType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "Dispatcher");
        }

        internal static Type GetServerProxyType(this Type interfaceType)
        {
            return GetAutoGeneratedType(interfaceType, "ServerProxy");
        }

        internal static string ResolveIdentifier(this Type interfaceType)
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

        private static Type GetAutoGeneratedType(Type interfaceType, string category)
        {
            string typeName;
            var prefix = /*AutoGeneratedPrefix +*/ category;

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
    }
}