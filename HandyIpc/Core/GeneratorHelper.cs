using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HandyIpc.Exceptions;

namespace HandyIpc.Core
{
    public static class GeneratorHelper
    {
        /// <summary>
        /// Unpack the result of the <see cref="Task"/> or <see cref="Task{T}"/> with the specified instance and the related type.
        /// </summary>
        /// <remarks>
        /// It is used to get the result from a invoked async generic method on server side.
        /// </remarks>
        /// <param name="taskType">The the <see cref="Task"/> or <see cref="Task{T}"/> type.</param>
        /// <param name="value">The instance of the value type.</param>
        /// <returns>The result from the value instance.</returns>
        public static async Task<object?> UnpackTask(Type taskType, object value)
        {
            if (value is not Task task)
            {
                throw new ArgumentException("This method accept Task type argument only.");
            }

            await task;
            return taskType.GetProperty("Result")?.GetMethod.Invoke(value, null);
        }

        public static Type ExtractTaskValueType(Type taskType)
        {
            return typeof(Task).IsAssignableFrom(taskType) &&
                   typeof(Task) != taskType
                ? taskType.GenericTypeArguments.Single()
                : taskType;
        }

        public static T UnpackResponse<T>(byte[] bytes, ISerializer serializer)
        {
            bool hasValue = Response.TryParse(bytes, typeof(T), serializer, out object? value, out Exception? exception);
            return hasValue ? (T)value! : throw new IpcException("An unexpected exception occurs during an ipc call.", exception!);
        }

        public static IReadOnlyDictionary<string, MethodInfo> GetGenericMethodMapping(Type interfaceType, object instance)
        {
            return instance.GetType()
                .GetInterfaceMap(interfaceType)
                .TargetMethods
                .Where(item => item.IsGenericMethod)
                .ToDictionary(item => item.GetCustomAttribute<IpcMethodAttribute>().Identifier);
        }
    }
}
