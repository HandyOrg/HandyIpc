using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace HandyIpcTests.Mock
{
    public class CollectionType
    {
        public IEnumerable<float> Floats { get; init; }

        public ConcurrentQueue<double> Doubles { get; init; }

        public HashSet<long> Longs { get; init; }

        public IReadOnlyList<int> Ints { get; init; }

        public IReadOnlyCollection<short> Shorts { get; init; }

        public List<ulong> Ulongs { get; init; }

        public IList<uint> Uints { get; init; }

        public ushort[] Ushorts { get; init; }

        public Collection<char> Chars { get; init; }

        public ObservableCollection<byte> Bytes { get; init; }

        public ImmutableArray<bool> Booleans { get; init; }
    }
}
