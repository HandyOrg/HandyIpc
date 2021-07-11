using System;
using System.Collections.Generic;

namespace HandyIpc
{
    public interface ISerializer
    {
        byte[] SerializeRequest(Request request, object?[]? arguments);

        byte[] SerializeResponse(Response response);

        Request DeserializeRequest(byte[] bytes);

        object?[]? DeserializeArguments(byte[] bytes, IReadOnlyList<Type> types);

        Response DeserializeResponse(byte[] bytes);
    }
}
