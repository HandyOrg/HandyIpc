using System.Collections.Generic;

namespace HandyIpc.BuildTasks
{
    public class TemplateData
    {
        public List<string> UsingList { get; set; }

        public List<ClassData> ClassList { get; set; }
    }

    public class ClassData
    {
        public string Namespace { get; set; } // Namespace
        public string Modifiers { get; set; } // public
        public string InterfaceName { get; set; } // Class.Interface
        public string GeneratedClassSuffix { get; set; } // Class.Interface --> ClassInterface
        //public List<BaseClassInfo> BaseClasses { get; set; } // TODO: Support inheritance.
        public string TypeParameters { get; set; } // T1, T2, T3
        public string TypeArguments { get; set; } // typeof(T1), typeof(T2), typeof(T3)
        public string ConstraintClauses { get; set; } // where T1: class where T2: new()
        public List<MethodData> MethodList { get; set; }
    }

    public class MethodData
    {
        public bool IsVoid { get; set; }
        public bool IsAwaitable { get; set; }
        public string ReturnType { get; set; } // void
        public string TaskReturnType { get; set; } // Task<T> --> T

        public string Name { get; set; } // MethodName

        public string Parameters { get; set; } // x, y, z
        public string ParameterTypes { get; set; } // string, int, double
        public string TypeAndParameters { get; set; } // string x, int y, double z
        public string Arguments { get; set; } // args[0].CastTo<string>(), args[1].CastTo<int>(), arg[2].CastTo<double>()
    }

    public class TypeData
    {
        public string Name { get; set; }
        public List<TypeData> Children { get; set; }
    }
}
