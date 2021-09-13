namespace HandyIpc
{
    public static class ContainerClientExtensions
    {
        public static T Resolve<T>(this IContainerClient client) => client.Resolve<T>(typeof(T).GetDefaultKey());
    }
}
