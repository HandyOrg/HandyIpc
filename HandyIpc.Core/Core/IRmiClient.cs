using System.Threading.Tasks;

namespace HandyIpc.Core
{
    public interface IRmiClient
    {
        byte[] Invoke(string identifier, byte[] requestBytes);

        Task<byte[]> InvokeAsync(string identifier, byte[] requestBytes);
    }
}
