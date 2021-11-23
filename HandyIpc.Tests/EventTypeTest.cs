using System;
using System.Threading.Tasks;
using HandyIpc;
using HandyIpcTests.Fixtures;
using HandyIpcTests.Interfaces;
using Xunit;

namespace HandyIpcTests
{
    [Collection(nameof(CollectionFixture))]
    public class EventTypeTest
    {
        private readonly NamedPipeFixture _namedPipeFixture;
        private readonly SocketFixture _socketFixture;

        public EventTypeTest(NamedPipeFixture namedPipeFixture, SocketFixture socketFixture)
        {
            _namedPipeFixture = namedPipeFixture;
            _socketFixture = socketFixture;
        }

        [Fact]
        public async Task TestEventHandlerWithSocket()
        {
            var instance = _socketFixture.Client.Resolve<IEventType>();
            await TestEventHandlerSubscribeAndUnsubscribe(instance);
        }

        [Fact]
        public async Task TestEventHandlerWithNamedPipe()
        {
            var instance = _namedPipeFixture.Client.Resolve<IEventType>();
            await TestEventHandlerSubscribeAndUnsubscribe(instance);
        }

        private static async Task TestEventHandlerSubscribeAndUnsubscribe(IEventType instance)
        {
            // Some issues will occur only when the number of tests is high.
            // In particular, it tests whether the event calls are synchronized.
            const int testCount = 10000;

            int count = 0;
            Task WrapAsAsync(IEventType source)
            {
                TaskCompletionSource tcs = new();
                source.Changed += OnChanged;
                source.RaiseChanged(EventArgs.Empty);
                return tcs.Task;

                void OnChanged(object? sender, EventArgs e)
                {
                    source.Changed -= OnChanged;
                    count++;
                    tcs.SetResult();
                }
            }

            for (int i = 0; i < testCount; i++)
            {
                await WrapAsAsync(instance);
                Assert.Equal(i + 1, count);
            }
        }
    }
}
