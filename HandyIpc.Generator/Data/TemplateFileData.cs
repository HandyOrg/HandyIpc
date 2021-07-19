using System.Collections.Generic;

namespace HandyIpc.Generator.Data
{
    public class TemplateFileData
    {
        public List<string> UsingList { get; }

        public List<ClassData> ClassList { get; }

        public TemplateFileData(List<string> usingList, List<ClassData> classList)
        {
            UsingList = usingList;
            ClassList = classList;
        }
    }
}
