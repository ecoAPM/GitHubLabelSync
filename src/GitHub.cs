using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLabelSync
{
	public class GitHub : IGitHub
	{
		private readonly IGitHubClient _client;
		private readonly Action<string> _setStatus;
		private readonly Action<string> _log;

		public GitHub(IGitHubClient client, Action<string> setStatus, Action<string> log)
		{
			_client = client;
			_setStatus = setStatus;
			_log = log;
		}

		public async Task<Account> GetOrganization(string name)
		{
			_setStatus($"Finding organization for {name}...");
			var account = await _client.Organization.Get(name);
			_log($"Organization ID for {name}: {account.Id}");
			return account;
		}

		public async Task<Account> GetUser(string name)
		{
			_setStatus($"Finding user for {name}...");
			var account = await _client.User.Get(name);
			_log($"User ID for {name}: {account.Id}");
			return account;
		}

		public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganization(Account account)
		{
			_setStatus($"Finding repositories for {account.Login}...");
			var repos = await _client.Repository.GetAllForOrg(account.Login);
			var repoNames = string.Join(", ", repos.Select(l => l.Name));
			_log($"{repos.Count} repositories for {account.Login}: {repoNames}");
			return repos;
		}

		public async Task<IReadOnlyList<Repository>> GetRepositoriesForUser(Account account)
		{
			_setStatus($"Finding repositories for {account.Login}...");
			var repos = await _client.Repository.GetAllForUser(account.Login);
			var repoNames = string.Join(", ", repos.Select(l => l.Name));
			_log($"{repos.Count} repositories for {account.Login}: {repoNames}");
			return repos;
		}

		public async Task<Repository> CreateTempRepoForOrganization(Account account, string repoName)
		{
			_setStatus($"Creating temp repository {repoName}...");
			var newRepo = new NewRepository(repoName) { Private = true };
			var repo = await _client.Repository.Create(account.Login, newRepo);
			_log($"Created temp repository {repoName}");
			return repo;
		}

		public async Task<Repository> CreateTempRepoForUser(Account account, string repoName)
		{
			_setStatus($"Creating temp repository {repoName}...");
			var newRepo = new NewRepository(repoName) { Private = true };
			var repo = await _client.Repository.Create(newRepo);
			_log($"Created temp repository {repoName}");
			return repo;
		}

		public async Task DeleteTempRepo(Account account, string repoName)
		{
			_setStatus($"Deleting temp repository {repoName}...");
			await _client.Repository.Delete(account.Login, repoName);
			_log($"Deleted temp repository {repoName}");
		}

		public async Task<IReadOnlyList<Label>> GetLabels(Repository repo)
		{
			_setStatus($"Finding labels for {repo.Name}...");
			var repoLabels = await _client.Issue.Labels.GetAllForRepository(repo.Id);
			var repoLabelNames = string.Join(", ", repoLabels.Select(l => l.Name));
			_log($"{repoLabels.Count,2} labels   : {repoLabelNames}");
			return repoLabels;
		}

		public async Task AddLabel(Repository repo, Label label)
		{
			_setStatus($"Adding {label.Name} to {repo.Name}...");
			var newLabel = new NewLabel(label.Name, label.Color) { Description = label.Description };
			await _client.Issue.Labels.Create(repo.Id, newLabel);
			_log($"Added {label.Name}");
		}

		public async Task EditLabel(Repository repo, Label label)
		{
			_setStatus($"Editing {label.Name} in {repo.Name}...");
			var newLabel = new LabelUpdate(label.Name, label.Color) { Description = label.Description };
			await _client.Issue.Labels.Update(repo.Id, label.Name, newLabel);
			_log($"Edited {label.Name}");
		}

		public async Task DeleteLabel(Repository repo, Label label)
		{
			_setStatus($"Deleting {label.Name} from {repo.Name}...");
			await _client.Issue.Labels.Delete(repo.Id, label.Name);
			_log($"Deleted {label.Name}");
		}
	}
}