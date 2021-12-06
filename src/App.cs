using System.Security.Authentication;
using System.Text.RegularExpressions;
using Octokit;

namespace GitHubLabelSync;

public class App
{
	private readonly ISynchronizer _sync;
	private readonly Action<string> _setStatus;
	private readonly Action<string> _log;

	public App(ISynchronizer sync, Action<string> setStatus, Action<string> log)
	{
		_sync = sync;
		_setStatus = setStatus;
		_log = log;
	}

	public async Task Run(Settings settings)
	{
		_setStatus($"Starting...");
		_log(settings.Name);

		var account = await GetValidAccount(settings);
		var allRepos = await _sync.GetRepositories(account);
		var labels = await _sync.GetAccountLabels(account);
		_log(string.Empty);

		var filteredRepos = settings.Filters.Any()
			? allRepos.Where(r => settings.Filters.Any(f => new Regex(f).IsMatch(r.Name)))
			: allRepos;

		var repos = filteredRepos.ToArray();
		if (!repos.Any())
		{
			_log("(no repositories to sync)");
		}

		foreach (var repo in repos)
		{
			await SyncRepo(repo, settings, labels);
		}

		_log("Done!");
	}

	private async Task SyncRepo(Repository repo, Settings settings, IReadOnlyList<Label> labels)
	{
		_log(repo.Name);

		if (repo.Archived)
		{
			_log($"(skipping: repo is archived)");
		}
		else
		{
			await _sync.SyncRepo(repo, settings, labels);
		}

		_log(string.Empty);
	}

	private async Task<Account> GetValidAccount(Settings settings)
	{
		var access = await _sync.ValidateAccess();
		if (!access.Successful)
		{
			throw new AuthenticationException(access.Message);
		}

		var account = await _sync.GetAccount(settings.Name);

		var auth = await _sync.ValidateUser(account);
		if (!auth.Successful)
		{
			throw new AuthenticationException(auth.Message);
		}

		return account;
	}
}