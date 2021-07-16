using HandyIpc;
using System;

namespace HandyIpcTests.Mock
{
    public class MockSerializer : ISerializer
    {
        public byte[] Serialize(object? value, Type type)
        {
            throw new NotImplementedException();
        }

        public object? Deserialize(byte[] bytes, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
