using System;

namespace HandyIpc
{
    public readonly struct Argument
    {
        public readonly Type Type;

        public readonly object? Value;

        public Argument(Type type, object? value)
        {
            Type = type;
            Value = value;
        }

        public void Deconstruct(out Type type, out object? value)
        {
            type = Type;
            value = Value;
        }
    }
}
