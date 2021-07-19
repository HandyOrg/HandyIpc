using System.Collections.Generic;

namespace HandyIpc.Generator.Data
{
    public class MethodData
    {
        public bool IsVoid { get; set; }

        public bool IsAwaitable { get; set; }

        public string ReturnType { get; set; } // void

        public string TaskReturnType { get; set; } // Task<string> --> string

        public bool TaskReturnTypeContainsGenericParameter { get; set; } // Task<List<T>> Method<T>()

        public string Name { get; set; } = null!; // MethodName

        public IReadOnlyList<string> Parameters { get; set; } = Common.EmptyStringList; // [x, y, z, ...]

        public IReadOnlyList<string> ParameterTypes { get; set; } = Common.EmptyStringList; // [string, int, double, ...]

        //public string TypeAndParameters { get; set; } // string x, int y, double z

        //public string Arguments { get; set; } // (string)args[0], (int)args[1], (double)arg[2]

        public IReadOnlyList<string> TypeParameters { get; set; } = Common.EmptyStringList; // [T1, T2, T3, ...]

        // The Type[] of method parameter must be sent to server from client when it is generic method,
        // if not, the data types cannot be converted correctly. e.g. Object of type 'System.Int64' cannot be converted to type 'System.Int32'.
        //public string MethodParameterTypes { get; set; } // typeof(string), typeof(List<T1>)
    }
}
