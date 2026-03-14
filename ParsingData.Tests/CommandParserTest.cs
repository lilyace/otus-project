using System.Text;

namespace ParsingData.Tests
{
    public class CommandParserTest
    {
        [Fact]
        public void Parsing_SetCommand_Correct()
        {
            //Arrange
            //Act
            var result = CommandParser.Parse(Encoding.UTF8.GetBytes("SET user 1"));
            //Assert
            Assert.Equal("SET", result.Command.ToString());
            Assert.Equal("user", result.Key.ToString());
            Assert.Equal("1", Encoding.UTF8.GetString(result.Value));
        }

        [Fact]
        public void Parsing_GetCommand_Correct()
        {
            //Arrange
            //Act
            var result = CommandParser.Parse(Encoding.UTF8.GetBytes("GET user1"));
            //Assert
            Assert.Equal("GET", result.Command.ToString());
            Assert.Equal("user1", result.Key.ToString());
            Assert.True(result.Value.IsEmpty);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("GET  user2")]
        public void Parsing_Command_Incorrect(string text)
        {
            //Arrange
            //Act
            var result = CommandParser.Parse(Encoding.UTF8.GetBytes(text));
            //Assert
            Assert.True(result.Command.IsEmpty);
            Assert.True(result.Key.IsEmpty);
            Assert.True(result.Value.IsEmpty);
        }
    }
}