using System;
using HandyIpcTests.Interfaces;

namespace HandyIpcTests.Implementations
{
    public class BuildInTypeTest : IBuildInTypeTest
    {
        public void TestVoidWithParams()
        {
            throw new NotImplementedException();
        }

        public void TestVoidWithBasicTypeParams(
            float @float,
            double @double,
            long @long,
            int @int,
            short @short,
            ulong @ulong,
            uint @uint,
            ushort @ushort,
            char @char,
            byte @byte)
        {
            throw new NotImplementedException();
        }

        public float TestFloat(float value) => value;

        public double TestDouble(double value) => value;

        public long TestLong(long value) => value;

        public int TestInt(int value) => value;

        public short TestShort(short value) => value;

        public ulong TestUlong(ulong value) => value;

        public uint TestUint(uint value) => value;

        public ushort TestUshort(ushort value) => value;

        public char TestChar(char value) => value;

        public byte TestByte(byte value) => value;

        public object TestNull(object value) => value;

        public byte[] TestByteArray(byte[] value) => value;
    }
}
