using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubLabelSync;

public class Command : AsyncCommand<Settings>
{
	private readonly IAnsiConsole _console;

	public Command(IAnsiConsole console)
		=> _console = console;

	public async Task<int> ExecuteAsync(CommandContext context, Settings settings)
		=> await ExecuteAsync(context, settings, CancellationToken.None);

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
	{
		try
		{
			await _console.Status().StartAsync("Running...", async ctx => await Run(ctx, settings));
			return 0;
		}
		catch (Exception e)
		{
			_console.WriteException(e, ExceptionFormats.ShortenEverything);
			return 1;
		}
	}

	private async Task Run(StatusContext ctx, Settings settings)
		=> await Factory
			.App(settings.APIKey, s => ctx.Status(s), s => _console.WriteLine(s))
			.Run(settings);
}