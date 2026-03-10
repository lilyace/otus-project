namespace ParsingData
{
    public ref struct DataInfo
    {
        public ReadOnlySpan<char> Command { get; private set; }
        public ReadOnlySpan<char> Key { get; private set; }
        public ReadOnlySpan<char> Value { get; private set; }

        public DataInfo(ReadOnlySpan<char> command, ReadOnlySpan<char> key, ReadOnlySpan<char> value)
        {
            Command = command;
            Key = key;
            Value = value;
        }
    }
}
