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
                /*
                 * BUG: 以下代码，若是严格以 ADD-RAISE-REMOVE 为一组执行，则是正常的，但由于 RAISE 是（非阻塞）异步的（仅用 Push 方法添加事件到队列），
                 * 故有可能造成这样的执行顺序：ADD1-RAISE1-ADD2-REMOVE1-RAISE2，其中 ADD2 由于 Dispatcher.g.cs 中的“引用计数”机制，是不会发送订阅事件的，
                 * 故 REMOVE1 将移除 Server 端的订阅，造成 Server 没有 connection 来处理 RAISE2 的消息，故 await WrapAsAsync() 将永远等待下去。。
                 */

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
