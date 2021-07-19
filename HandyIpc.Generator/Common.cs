using System.Collections.Generic;
using System.Linq;

namespace HandyIpc.Generator
{
    public static class Common
    {
        public static readonly IReadOnlyList<string> EmptyStringList = Enumerable.Empty<string>().ToList();
    }
}
