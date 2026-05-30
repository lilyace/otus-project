//using ParsingData.Generators;

namespace ParsingData
{
    [GenerateBinarySerializer]
    public partial class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }

       // public List<string> TodoList { get; set; }
    }
}
