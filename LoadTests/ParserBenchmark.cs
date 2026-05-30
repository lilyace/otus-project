using BenchmarkDotNet.Attributes;
using ParsingData;
using Spectre.Console;
using System.Text;
using System.Text.Json;

namespace LoadTests
{
    [MemoryDiagnoser]
    public class ParserBenchmark
    {
        private byte[] _data;

        [GlobalSetup]
        public void GlobalSetup()
        {     
            _data = JsonSerializer.SerializeToUtf8Bytes("SET Test TestTestTestTestTestTestTestTest");
        }

        [Benchmark(Baseline = true, Description = "Parser with span")]
        public void ParseWithSpanParser()
        {
            var result = CommandParser.Parse(_data.AsSpan());
        }

        [Benchmark(Description = "Parser with arrays")]
        public void ParseWithArrayParser()
        {
            var result = CommandParserWithArrays.Parse(_data);
        }
    }
}
