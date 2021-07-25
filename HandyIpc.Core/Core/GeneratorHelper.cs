using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        /// <param name="task">The instance of the task type.</param>
        /// <returns>The result from the task instance.</returns>
        public static async Task<object?> UnpackTask(Type taskType, object task)
        {
            if (task is not Task taskInstance) return null;

            await taskInstance;
            return taskType.GetProperty("Result")?.GetMethod.Invoke(task, null);
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
