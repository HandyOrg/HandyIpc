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
        public void TestEventHandlerWithSocket()
        {
            var instance = _socketFixture.Client.Resolve<IEventType>();
            TestEventHandlerSubscribeAndUnsubscribe(instance);
        }

        [Fact]
        public void TestEventHandlerWithNamedPipe()
        {
            var instance = _namedPipeFixture.Client.Resolve<IEventType>();
            TestEventHandlerSubscribeAndUnsubscribe(instance);
        }

        private void TestEventHandlerSubscribeAndUnsubscribe(IEventType instance)
        {
            int count1 = 0;
            int count2 = 0;
            int count3 = 0;

            // ReSharper disable AccessToModifiedClosure
            void Handler1(object? _, EventArgs e) => count1++;
            EventHandler handler2 = (_, _) => count2++;
            EventHandler handler3 = (_, _) => count3++;
            // ReSharper restore AccessToModifiedClosure

            instance.Changed += Handler1;
            instance.Changed += handler2;
            instance.Changed += handler3;

            for (int i = 0; i < 10; i++)
            {
                instance.RaiseChanged(EventArgs.Empty);
                Assert.Equal(i + 1, count1);
                Assert.Equal(i + 1, count2);
                Assert.Equal(i + 1, count3);
            }

            count1 = 0;
            count2 = 0;
            count3 = 0;

            instance.Changed -= Handler1;
            instance.Changed -= handler2;
            instance.Changed -= handler3;

            for (int i = 0; i < 10; i++)
            {
                instance.RaiseChanged(EventArgs.Empty);
                Assert.Equal(0, count1);
                Assert.Equal(0, count2);
                Assert.Equal(0, count3);
            }

            instance.Changed += Handler1;
            instance.Changed += Handler1;
            instance.Changed += handler2;
            instance.Changed += handler2;
            instance.Changed += handler3;

            for (int i = 0; i < 10; i++)
            {
                instance.RaiseChanged(EventArgs.Empty);
                Assert.Equal(2 * (i + 1), count1);
                Assert.Equal(2 * (i + 1), count2);
                Assert.Equal(i + 1, count3);
            }
        }
    }
}
