namespace HandyIpc.BuildTasks.Data
{
    public class MethodData
    {
        public bool IsVoid { get; set; }

        public bool IsAwaitable { get; set; }

        public string ReturnType { get; set; } // void

        public string TaskReturnType { get; set; } // Task<string> --> string

        public bool TaskReturnTypeContainsGenericParameter { get; set; } // Task<List<T>> Method<T>()

        public string Name { get; set; } // MethodName

        public string Parameters { get; set; } // x, y, z

        public string ParameterTypes { get; set; } // string, int, double

        public string TypeAndParameters { get; set; } // string x, int y, double z

        public string Arguments { get; set; } // (string)args[0], (int)args[1], (double)arg[2]

        public string MethodTypeParameters { get; set; } // T1, T2, T3

        public string MethodTypeArguments { get; set; } // typeof(T1), typeof(T2), typeof(T3)

        // TODO: Delete this property because it is useless. 
        // When an interface is implicitly implemented,
        // it automatically inherits the generic constraints from the interface
        // and does not have to be declared again itself.
        public string MethodConstraintClauses { get; set; } // where T1: class where T2: new()

        // The Type[] of method parameter must be sent to server from client when it is generic method,
        // if not, the data types cannot be converted correctly. e.g. Object of type 'System.Int64' cannot be converted to type 'System.Int32'.
        public string MethodParameterTypes { get; set; } // typeof(string), typeof(List<T1>)
    }
}
