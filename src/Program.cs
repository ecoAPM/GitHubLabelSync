using Spectre.Console.Cli;

namespace GitHubLabelSync;

public static class Program
{
	public static async Task<int> Main(string[] args)
		=> await new CommandApp<Command>().RunAsync(args);
}