namespace HandyIpc
{
    public interface ISerializer
    {
        byte[] SerializeRequest(Request request);

        byte[] SerializeResponse(Response response);

        Request DeserializeRequest(byte[] bytes);

        Response DeserializeResponse(byte[] bytes);
    }
}
