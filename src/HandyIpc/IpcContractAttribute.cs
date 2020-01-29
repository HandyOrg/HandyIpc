using System;

namespace HandyIpc
{
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class IpcContractAttribute : Attribute
    {
        public string Identifier { get; set; }

        public string AccessToken { get; set; }
    }
}
