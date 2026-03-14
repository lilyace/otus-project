namespace ParsingData
{
    public ref struct DataInfo
    {
        public ReadOnlySpan<char> Command { get; private set; }
        public ReadOnlySpan<char> Key { get; private set; }
        public ReadOnlySpan<byte> Value { get; private set; }

        public DataInfo(ReadOnlySpan<char> command, ReadOnlySpan<char> key, ReadOnlySpan<byte> value)
        {
            Command = command;
            Key = key;
            Value = value;
        }
    }
}
