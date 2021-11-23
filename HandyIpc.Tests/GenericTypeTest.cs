using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandyIpc;
using HandyIpcTests.Fixtures;
using HandyIpcTests.Implementations;
using HandyIpcTests.Interfaces;
using HandyIpcTests.Mock;
using Xunit;

namespace HandyIpcTests
{
    [Collection(nameof(CollectionFixture))]
    public class GenericTypeTest
    {
        private readonly NamedPipeFixture _namedPipeFixture;
        private readonly SocketFixture _socketFixture;

        public GenericTypeTest(NamedPipeFixture namedPipeFixture, SocketFixture socketFixture)
        {
            _namedPipeFixture = namedPipeFixture;
            _socketFixture = socketFixture;
        }

        [Fact]
        public Task TestBuildInTypesWithNamedPipe()
        {
            var instance = _socketFixture.Client.Resolve<IGenericType<ClassWithNewCtor, string>>();
            return TestCases(instance);
        }

        [Fact]
        public Task TestBuildInTypesWithSocket()
        {
            var instance = _namedPipeFixture.Client.Resolve<IGenericType<ClassWithNewCtor, string>>();
            return TestCases(instance);
        }

        public async Task TestCases(IGenericType<ClassWithNewCtor, string> remote)
        {
            var local = new GenericTypeImpl<ClassWithNewCtor, string>();

            {
                const string expected = "first";
                string actual = remote.First(new[] { expected, "other" });
                Assert.Equal(expected, actual);
            }

            await Helper.AssertInnerException<TestException>(remote.TestAsync);

            {
                ClassWithNewCtor.InitialName = Guid.NewGuid().ToString();
                var actual = await remote.TestAsync(string.Empty);

                Assert.Equal(ClassWithNewCtor.InitialName, actual.Name);
            }

            {
                string expected = local.TestTypeOf<string, BuildInType>();
                string actual = remote.TestTypeOf<string, BuildInType>();

                Assert.Equal(expected, actual);
            }

            {
                var expected = new ClassWithNewCtor
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                };
                var actual = remote.TestGenericConstraint(expected);

                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
            }

            {
                var expected = new ClassWithNewCtor
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                };
                var actual = remote.ReturnOriginalValue(expected);

                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
            }

            {
                var actual = remote.Flatten(new List<List<List<string>>>
                {
                    new()
                    {
                        new() { "1", "2", "3", },
                        new() { "4", "5", "6", },
                    },
                    new()
                    {
                        new() { "7", "8", "9", },
                        new() { "10", "11", "12", },
                    },
                });

                Assert.Equal(Enumerable.Range(1, 12).Select(item => $"{item}"), actual);
            }

            {
                var expected = new List<string> { "1", "2", "3", };
                var actual = remote.SendList(expected);

                Assert.Equal(expected.AsEnumerable(), actual);
            }

            {
                var arg1 = new ClassWithNewCtor
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString()
                };
                string arg2 = Guid.NewGuid().ToString();

                string expected = local.PrintGenericArguments(arg1, arg2);
                string actual = remote.PrintGenericArguments(arg1, arg2);

                Assert.Equal(expected, actual);
            }
        }
    }
}
