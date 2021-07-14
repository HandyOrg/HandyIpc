using HandyIpc;

namespace HandyIpcTests.Interfaces
{
    [IpcContract(Identifier = nameof(IBuildInTypeTest))]
    public interface IBuildInTypeTest
    {
        void TestVoidWithParams();

        void TestVoidWithBasicTypeParams(
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

        float TestFloat(float @float);

        double TestDouble(double @double);

        long TestLong(long @long);

        int TestInt(int @int);

        short TestShort(short @short);

        ulong TestUlong(ulong @ulong);

        uint TestUint(uint @uint);

        ushort TestUshort(ushort @ushort);

        char TestChar(char @char);

        byte TestByte(byte @byte);
    }
}
