using System;

namespace HandyIpc.Core
{
    public interface ISerializer
    {
        byte[] Serialize(object? value, Type type);

        object? Deserialize(byte[] bytes, Type type);
    }
}
