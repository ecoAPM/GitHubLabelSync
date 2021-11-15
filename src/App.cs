using System;
using System.Security.Authentication;
using System.Threading.Tasks;
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
		var repos = await _sync.GetRepositories(account);
		var labels = await _sync.GetAccountLabels(account);
		_log(string.Empty);

		foreach (var repo in repos)
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

		_log("Done!");
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
