using System;
using System.Diagnostics;
using System.Text;

namespace HandyIpc.Core
{
    public enum SubscriptionType { Add, Promise, Remove }

    public class Subscription
    {
        private const string CallbackHeader = "handyipc/cb";

        private static readonly int InnerProcessId = Process.GetCurrentProcess().Id;
        private static readonly byte[] CallbackHeaderBytes = Encoding.ASCII.GetBytes(CallbackHeader);

        public SubscriptionType Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public string CallbackName { get; set; } = string.Empty;

        public int ProcessId { get; } = InnerProcessId;

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

        internal static byte[] Promise(string key, string name, ISerializer serializer)
        {
            return GetBytes(SubscriptionType.Promise, key, name, serializer);
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
