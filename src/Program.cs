using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace GitHubLabelSync
{
	public static class Program
	{
		public static async Task Main(string[] args)
			=> await new CommandApp<Command>().RunAsync(args);
	}
}