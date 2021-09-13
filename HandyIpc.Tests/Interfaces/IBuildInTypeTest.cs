using HandyIpc;

namespace HandyIpcTests.Interfaces
{
    [IpcContract]
    public interface IBuildInTypeTest
    {
        void TestVoidWithParams();

        string TestVoidWithBasicTypeParams(
            float @float,
            double @double,
            long @long,
            int @int,
            short @short,
            ulong @ulong,
            uint @uint,
            ushort @ushort,
            char @char,
            byte @byte);

        float TestFloat(float value);

        double TestDouble(double value);

        long TestLong(long value);

        int TestInt(int value);

        short TestShort(short value);

        ulong TestUlong(ulong value);

        uint TestUint(uint value);

        ushort TestUshort(ushort value);

        char TestChar(char value);

        byte TestByte(byte value);

        object? TestNull(object? value);

        byte[] TestByteArray(byte[] value);
    }
}
