using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandyIpc.Tests.ContractInterfaces;

namespace HandyIpc.Tests.Impls
{
    internal class GenericMethods : IGenericMethods
    {
        public Task<T1> PrintAsync<T1, T2>(List<T2> items1, List<List<List<T1>>> items2)
        {
            return Task.FromResult(items2.First().First().FirstOrDefault());
        }

        public Task<string> PrintAsync<T1, T2, T3>()
        {
            return Task.FromResult($"{typeof(T1)}, {typeof(T2)}, {typeof(T3)}");
        }

        public Task<IEnumerable<T>> RepeatAsync<T>(T item, int count)
        {
            return Task.FromResult<IEnumerable<T>>(Enumerable.Repeat(item, count).ToList().AsReadOnly());
        }
    }
}
