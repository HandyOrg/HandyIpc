using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HandyIpc.Socket
{
    public static class Extensions
    {
        private const int BatchBufferSize = 4 * 1024;

        private static readonly char[] IdentifierSplitter = { ':', ' ' };

        public static IConfiguration UseTcp(this IConfiguration self, IPAddress ip, int port)
        {
            return self
                .Use(() => new TcpSender(ip, port))
                .Use(() => new TcpReceiver(ip, port));
        }

        internal static (IPAddress ip, int port) ToIpEndPoint(this string connectionString)
        {
            const int ipIndex = 0;
            const int portIndex = 1;

            string[] address = connectionString.Split(IdentifierSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (address.Length != 2)
            {
                throw new FormatException($"Invalid identifier. The correct format is 'x.x.x.x:port', rather than '{connectionString}'");
            }

            return (IPAddress.Parse(address[ipIndex]), int.Parse(address[portIndex]));
        }

        internal static byte[] ReadAllBytes(this Stream self)
        {
            // TODO: Refactoring by System.IO.Pipelines, ArrayPool or stackalloc and so on.
            var collector = new List<byte[]>();
            while (true)
            {
                byte[] bytes = new byte[BatchBufferSize];
                int actualCount = self.Read(bytes, 0, BatchBufferSize);

                if (CollectBytes(collector, bytes, actualCount))
                {
                    break;
                }
            }

            return ConcatBytesList(collector);
        }

        internal static async Task<byte[]> ReadAllBytesAsync(this Stream self, CancellationToken token)
        {
            // TODO: Refactoring by System.IO.Pipelines, ArrayPool or stackalloc and so on.
            var collector = new List<byte[]>();
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                byte[] bytes = new byte[BatchBufferSize];
                int actualCount = await self.ReadAsync(bytes, 0, BatchBufferSize, token);

                if (CollectBytes(collector, bytes, actualCount))
                {
                    break;
                }
            }

            return ConcatBytesList(collector);
        }

        /// <summary>
        /// Fill a bytes list, and return a bool value to indicate whether the process has been completed. <br />
        /// I known this method is so ugly: It modifies external collection, but it was extracted to be able to
        /// reuse code in async and sync methods.
        /// </summary>
        /// <param name="collector">The filled bytes list.</param>
        /// <param name="bytes">The buffered bytes.</param>
        /// <param name="actualCount">The actual length of the buffered bytes (<see cref="bytes"/>).</param>
        /// <returns>The bool value indicate whether it has been completed.</returns>
        private static bool CollectBytes(ICollection<byte[]> collector, byte[] bytes, int actualCount)
        {
            if (actualCount == 0)
            {
                return true;
            }

            if (actualCount < BatchBufferSize)
            {
                byte[] tailBytes = new byte[actualCount];
                Array.Copy(bytes, tailBytes, actualCount);
                collector.Add(tailBytes);
                return true;
            }

            collector.Add(bytes);
            return false;
        }

        private static byte[] ConcatBytesList(IReadOnlyList<byte[]> bytesList)
        {
            int totalSize = bytesList.Sum(item => item.Length);
            byte[] totalBytes = new byte[totalSize];
            int offset = 0;
            // "for" performance is higher than "foreach".
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < bytesList.Count; i++)
            {
                byte[] itemBytes = bytesList[i];
                itemBytes.CopyTo(totalBytes, offset);
                offset += itemBytes.Length;
            }

            return totalBytes;
        }
    }
}
