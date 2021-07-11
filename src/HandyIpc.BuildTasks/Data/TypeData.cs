using System.Collections.Generic;

namespace HandyIpc.BuildTasks.Data
{
    public class TypeData
    {
        public string Name { get; }

        public List<TypeData>? Children { get; set; }

        public TypeData(string name)
        {
            Name = name;
        }
    }
}
