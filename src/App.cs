using System;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace GitHubLabelSync
{
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

			var validation = await _sync.ValidateAccess();
			if(!validation.Successful)
			{
				throw new AuthenticationException(validation.Message);
			}

			var account = await _sync.GetAccount(settings.Name);
			_log(string.Empty);

			var repos = await _sync.GetRepositories(account);
			_log(string.Empty);

			var labels = await _sync.GetAccountLabels(account);
			_log(string.Empty);

			foreach (var repo in repos)
			{
				await _sync.SyncRepo(repo, settings, labels);
				_log(string.Empty);
			}

			_log("Done!");
		}
	}
}