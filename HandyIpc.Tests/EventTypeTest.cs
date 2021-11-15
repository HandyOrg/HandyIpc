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
        public void TestEventHandler()
        {
            int count = 0;
            var instance = _socketFixture.Client.Resolve<IEventType>();
            instance.Changed += Instance_Changed;
            instance.Changed += (sender, e) => count++;

            instance.RaiseChanged(EventArgs.Empty);
            instance.RaiseChanged(EventArgs.Empty);
            instance.RaiseChanged(EventArgs.Empty);
            instance.RaiseChanged(EventArgs.Empty);

            Assert.Equal(4, count);
        }

        private void Instance_Changed(object? sender, EventArgs e)
        {

        }
    }
}
