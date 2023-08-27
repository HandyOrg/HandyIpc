using System.Diagnostics;
using System.Text;

namespace HandyIpc.Core
{
    public enum SubscriptionType { Add, Remove }

    public class Subscription
    {
        private const string CallbackHeader = "hi/cb";

        private static readonly byte[] CallbackHeaderBytes = Encoding.ASCII.GetBytes(CallbackHeader);
        private static readonly int ProcessIdSelf = Process.GetCurrentProcess().Id;

        public SubscriptionType Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CallbackName { get; set; } = string.Empty;

        public int ProcessId { get; set; }

        internal static bool TryParse(byte[] bytes, ISerializer serializer, out Subscription subscription)
        {
            subscription = null!;
            if (!CallbackHeaderBytes.EqualsHeaderBytes(bytes))
            {
                return false;
            }

            if (serializer.Deserialize(
                bytes.Slice(CallbackHeaderBytes.Length, bytes.Length - CallbackHeaderBytes.Length),
                typeof(Subscription)) is not Subscription result)
            {
                return false;
            }

            subscription = result;
            return true;
        }

        internal static byte[] Add(string key, string name, ISerializer serializer)
        {
            return GetBytes(SubscriptionType.Add, key, name, serializer);
        }

        internal static byte[] Remove(string key, string name, ISerializer serializer)
        {
            return GetBytes(SubscriptionType.Remove, key, name, serializer);
        }

        private static byte[] GetBytes(SubscriptionType type, string key, string name, ISerializer serializer)
        {
            byte[] payload = serializer.Serialize(new Subscription
            {
                Type = type,
                Name = key,
                ProcessId = ProcessIdSelf,
                CallbackName = name,
            }, typeof(Subscription));

            return new byte[][]
            {
                CallbackHeaderBytes,
                payload,
            }.ConcatBytes();
        }
    }
}
