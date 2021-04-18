using System;

namespace HandyIpc
{
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class IpcContractAttribute : Attribute
    {
        public string Identifier { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;
    }
}
