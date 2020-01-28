using System;

namespace HandyIpc
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
