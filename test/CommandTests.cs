using System.Threading.Tasks;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Cli;
using Xunit;

namespace GitHubLabelSync.Tests
{
    public class CommandTests
    {
        [Fact]
        public async Task RunPassesByDefault()
        {
            //arrange
            var console = Substitute.For<IAnsiConsole>();
            var command = new Command(console);

            var remaining = Substitute.For<IRemainingArguments>();
            var context = new CommandContext(remaining, "test", null);
            
            var settings = new Settings { APIKey = "abc123" };
            
            //act
            var result = await command.ExecuteAsync(context, settings);
            
            //assert
            Assert.Equal(0, result);
        }
    }
}