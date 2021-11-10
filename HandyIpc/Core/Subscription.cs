using System;
using System.Collections.Generic;
using System.Text;

namespace HandyIpc.Core
{
    public class Subscription
    {
        private const string AddHeader = "handyipc/cb/add";
        private const string RemoveHeader = "handyipc/cb/remove";

        public string Name { get; set; }

        public string CallbackName { get; set; }

        public string CallbackId { get; set; }

        internal static bool TryParse(byte[] input, ISerializer serializer, out Subscription subscription)
        {
            throw new NotImplementedException();
        }
    }
}
