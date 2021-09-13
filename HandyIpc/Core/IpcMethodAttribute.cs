using System;

namespace HandyIpc.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IpcMethodAttribute : Attribute
    {
        public string Identifier { get; }

        public IpcMethodAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}
