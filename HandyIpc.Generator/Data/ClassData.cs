using System.Collections.Generic;
using System.Linq;

namespace HandyIpc.Generator.Data
{
    public class ClassData
    {
        /// <summary>
        /// The namespace where the original ipc contract interface is located.
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Name of the original ipc contract interface.
        /// </summary>
        public string InterfaceName { get; set; } = string.Empty;

        /// <summary>
        /// The suffix of the class automatically generated from the interface name.
        /// </summary>
        /// <remarks>
        /// Looks like:  Class.Interface --> ClassInterface.
        /// </remarks>
        public string GeneratedClassSuffix { get; set; } = string.Empty;

        /// <summary>
        /// Generic parameters of the interface.
        /// </summary>
        /// <remarks>
        /// Looks like: [T1, T2, T3, ...]
        /// </remarks>
        public IReadOnlyList<string> TypeParameters { get; set; } = Common.EmptyStringList;

        /// <summary>
        /// Generalized Constraint Statements.
        /// </summary>
        /// <remarks>
        /// Looks like: where T1: class where T2: new()
        /// </remarks>
        public string ConstraintClauses { get; set; } = string.Empty;

        /// <summary>
        /// Determine if there are methods in the interface that contain generic parameters.
        /// </summary>
        public bool HasGenericMethod => MethodList.Any(item => item.ParameterTypes.Any());

        /// <summary>
        /// Ipc method list defined in the ipc contract interface.
        /// </summary>
        public IReadOnlyList<MethodData> MethodList { get; set; } = null!;
    }
}
