using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Spectre.Console;
using Spectre.Console.Cli;
using Xunit;

namespace GitHubLabelSync.Tests;

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

		var settings = new Settings { APIKey = "abc123", Name = "ecoAPM" };

		//act
		var result = await command.ExecuteAsync(context, settings);

		//assert
		Assert.Equal(0, result);
	}

	[Fact]
	public async Task RunFailsOnException()
	{
		//arrange
		var console = Substitute.For<IAnsiConsole>();
		console.ExclusivityMode.When(m => m.RunAsync(Arg.Any<Func<Task<object>>>()))
			.Do(_ => throw new Exception());
		var command = new Command(console);

		var remaining = Substitute.For<IRemainingArguments>();
		var context = new CommandContext(remaining, "test", null);

		var settings = new Settings { APIKey = "abc123", Name = "ecoAPM" };

		//act
		var result = await command.ExecuteAsync(context, settings);

		//assert
		Assert.Equal(1, result);
	}
}