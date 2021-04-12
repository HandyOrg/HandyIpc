namespace HandyIpc.Client
{
    public abstract class IpcClient
    {
        protected string Identifier { get; }

        protected string AccessToken { get; }

        protected IpcClient(string identifier, string accessToken)
        {
            Identifier = identifier;
            AccessToken = accessToken;
        }
    }
}
