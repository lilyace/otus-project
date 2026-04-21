using BenchmarkDotNet.Attributes;
using ParsingData;
using System.Text.Json;

namespace LoadTests
{
    [MemoryDiagnoser]
    public class BinarySerializerBenchmark
    {
        private UserProfile profile;

        [GlobalSetup]
        public void GlobalSetup()
        {
            profile = new UserProfile
            {
                Id = 1,
                Username = "Test",
                CreatedAt = DateTime.Now,
            };
        }

        [Benchmark(Baseline = true, Description = "Text.Json serialization")]
        public void TextJsonSerialization()
        {  
            JsonSerializer.SerializeToUtf8Bytes(profile);
        }

        [Benchmark(Description = "Binary serialization")]
        public void BinarySerialization()
        {
            using (var stream = new MemoryStream())
            {
                profile.SerializeToBinary(stream);
            }
        }
    }
}
