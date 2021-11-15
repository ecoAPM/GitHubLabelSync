using System;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubLabelSync;

public class Command : AsyncCommand<Settings>
{
	private readonly IAnsiConsole _console;

	public Command(IAnsiConsole console)
		=> _console = console;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		try
		{
			await _console.Status().StartAsync("Running...", Run(settings));
			return 0;
		}
		catch (Exception e)
		{
			_console.WriteException(e, ExceptionFormats.ShortenEverything);
			return 1;
		}
	}

	private Func<StatusContext, Task> Run(Settings settings)
		=> async ctx
			=> await Factory
				.App(settings.APIKey, s => ctx.Status(s), s => _console.WriteLine(s))
				.Run(settings);
}
