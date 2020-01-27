namespace HandyIpc
{
    public static class DataConstants
    {
        public static readonly byte[] Empty = { 0 };
        public static readonly byte[] Unit = { 1 };

        public static bool IsEmpty(this byte[] bytes)
        {
            return bytes.Length == 1 && bytes[0] == Empty[0];
        }

        public static bool IsUnit(this byte[] bytes)
        {
            return bytes.Length == 1 && bytes[0] == Unit[0];
        }
    }
}
