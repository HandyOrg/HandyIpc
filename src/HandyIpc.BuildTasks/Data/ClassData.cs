using System.Collections.Generic;
using System.Linq;

namespace HandyIpc.BuildTasks.Data
{
    public class ClassData
    {
        public string Namespace { get; set; } // like: Namespace

        public string Modifiers { get; set; } // like: public, internal

        public string InterfaceName { get; set; } // like: Class.Interface

        public string GeneratedClassSuffix { get; set; } // like: Class.Interface --> ClassInterface

        public string TypeParameters { get; set; } // like: T1, T2, T3

        public string TypeArguments { get; set; } // like: typeof(T1), typeof(T2), typeof(T3)

        public string ConstraintClauses { get; set; } // like:  where T1: class where T2: new()

        public bool HasGenericMethod => MethodList.Any(item => !string.IsNullOrEmpty(item.MethodTypeParameters));

        public List<MethodData> MethodList { get; set; }
    }
}
