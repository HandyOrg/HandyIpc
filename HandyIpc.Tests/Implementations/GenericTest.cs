﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests.Implementations
{
    public class GenericTest<T1, T2, T3> : IGenericTest<T1, T2, T3>
        where T2 : T3
        where T1 : class
    {
        public void TestVoid()
        {
            throw new NotImplementedException();
        }

        public T1 TestGenericT1(T1 value)
        {
            throw new NotImplementedException();
        }

        public T1 TestGenericT1(T1 value1, T2 value2)
        {
            throw new NotImplementedException();
        }

        public TM TestGenericMethod<TM>(TM value) where TM : new()
        {
            throw new NotImplementedException();
        }

        public List<TM> TestGenericMethod<TM>(List<TM> value)
        {
            throw new NotImplementedException();
        }

        public TM TestGenericMethod<TM>(TM[] value)
        {
            throw new NotImplementedException();
        }

        public List<TM> TestNestedGeneric<TM>(List<List<List<TM>>> toFlatten)
        {
            throw new NotImplementedException();
        }

        public void TestTypeOf<T>()
        {
            throw new NotImplementedException();
        }

        public string TestTypeOf<T, U>()
        {
            throw new NotImplementedException();
        }

        public async Task TestAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<T1> TestAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
