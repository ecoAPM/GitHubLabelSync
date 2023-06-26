using Octokit;
using Spectre.Console;

namespace GitHubLabelSync;

public class Synchronizer : ISynchronizer
{
	private readonly IGitHub _gitHub;
	private readonly Action<string> _setStatus;
	private readonly Action<string> _log;

	public Synchronizer(IGitHub gitHub, Action<string> setStatus, Action<string> log)
	{
		_gitHub = gitHub;
		_setStatus = setStatus;
		_log = log;
	}

	public async Task<ValidationResult> ValidateAccess()
	{
		var access = await _gitHub.GetAccess();

		if (access.Contains("repo") && access.Contains("delete_repo"))
		{
			return ValidationResult.Success();
		}

		var error = !access.Contains("repo")
			? "API key does not have `repo` access"
			: "API key does not have `delete_repo` access";
		return ValidationResult.Error(error);
	}

	public async Task<ValidationResult> ValidateUser(Account account)
	{
		var current = await _gitHub.GetCurrentUser();

		if (account.Type == AccountType.User)
			return current.Login == account.Login
				? ValidationResult.Success()
				: ValidationResult.Error($"API key for {current.Login} does not match target user {account.Login}");

		var role = await _gitHub.GetRole(current.Login, account.Login);
		return role == MembershipRole.Admin
			? ValidationResult.Success()
			: ValidationResult.Error($"{current.Login} is not an administrator of {account.Login}");
	}

	public async Task<Account> GetAccount(string name)
	{
		_setStatus($"Finding information for {name}...");
		try
		{
			return await _gitHub.GetOrganization(name);
		}
		catch
		{
			return await _gitHub.GetUser(name);
		}
	}

	public async Task<IReadOnlyList<Repository>> GetRepositories(Account account)
		=> account.Type == AccountType.Organization
			? await _gitHub.GetRepositoriesForOrganization(account)
			: await _gitHub.GetRepositoriesForUser(account);

	public async Task<IReadOnlyList<Label>> GetAccountLabels(Account account)
	{
		_setStatus($"Finding labels for {account.Login}...");

		var now = DateTime.Now.ToString("yyyyMMdd-HHmmssff");
		var repoName = $"temp-label-sync-{now}";

		var repo = account.Type == AccountType.Organization
			? await _gitHub.CreateTempRepoForOrganization(account, repoName)
			: await _gitHub.CreateTempRepoForUser(account, repoName);

		var originalLabels = await _gitHub.GetLabels(repo);
		var latestLabels = await _gitHub.GetLabels(repo);
		while (originalLabels.Count != latestLabels.Count)
		{
			originalLabels = latestLabels;
			latestLabels = await _gitHub.GetLabels(repo);
		}
		await _gitHub.DeleteTempRepo(account, repoName);

		return latestLabels;
	}

	private static string LabelNames(IEnumerable<Label> labels)
		=> string.Join(", ", labels.Select(l => l.Name));

	public async Task SyncRepo(Repository repo, Settings settings, IReadOnlyList<Label> accountLabels)
	{
		var repoLabels = await _gitHub.GetLabels(repo);

		ShowSynchronizedLabels(repo, accountLabels, repoLabels);
		var newLabels = GetLabelsToAdd(repo, accountLabels, repoLabels);
		var matchingLabels = GetLabelsToEdit(repo, accountLabels, repoLabels);
		var oldLabels = GetLabelsToDelete(repo, accountLabels, repoLabels);

		if (settings.Add)
		{
			await AddLabels(repo, newLabels);
		}

		if (settings.Edit)
		{
			await EditLabels(repo, matchingLabels);
		}

		if (settings.Delete)
		{
			await DeleteLabels(repo, oldLabels);
		}
	}

	private void ShowSynchronizedLabels(Repository repo, IEnumerable<Label> accountLabels, IEnumerable<Label> repoLabels)
	{
		_setStatus($"Finding synchronized labels in {repo.Name}...");
		var matchingLabels = accountLabels.Where(al => repoLabels.Any(rl => Matching(al, rl))).ToArray();
		_log($"{matchingLabels.Length,3} {"sync'd",-9} : {LabelNames(matchingLabels)}");
	}

	private IEnumerable<Label> GetLabelsToAdd(Repository repo, IEnumerable<Label> accountLabels, IEnumerable<Label> repoLabels)
	{
		_setStatus($"Finding labels to add to {repo.Name}...");
		var newLabels = accountLabels.Where(al => repoLabels
				.All(rl => !string.Equals(rl.Name, al.Name, StringComparison.CurrentCultureIgnoreCase)
						   && !string.Equals(rl.Description, al.Description, StringComparison.CurrentCultureIgnoreCase)))
			.ToArray();
		_log($"{newLabels.Length,3} {"to add",-9} : {LabelNames(newLabels)}");
		return newLabels;
	}

	private IEnumerable<Label> GetLabelsToEdit(Repository repo, IEnumerable<Label> accountLabels, IEnumerable<Label> repoLabels)
	{
		_setStatus($"Finding labels to edit in {repo.Name}...");
		var editLabels = accountLabels.Where(al => repoLabels.Any(rl => NeedsUpdating(al, rl))).ToArray();
		_log($"{editLabels.Length,3} {"to edit",-9} : {LabelNames(editLabels)}");
		return editLabels;
	}

	private IEnumerable<Label> GetLabelsToDelete(Repository repo, IEnumerable<Label> accountLabels, IEnumerable<Label> repoLabels)
	{
		_setStatus($"Finding labels to delete from {repo.Name}...");
		var oldLabels = repoLabels.Where(rl => accountLabels
				.All(al => !string.Equals(al.Name, rl.Name, StringComparison.InvariantCultureIgnoreCase)
						   && !string.Equals(al.Description, rl.Description, StringComparison.InvariantCultureIgnoreCase)))
			.ToArray();
		_log($"{oldLabels.Length,3} {"to delete",-9} : {LabelNames(oldLabels)}");
		return oldLabels;
	}

	private async Task AddLabels(Repository repo, IEnumerable<Label> newLabels)
	{
		foreach (var label in newLabels)
		{
			await _gitHub.AddLabel(repo, label);
		}
	}

	private async Task EditLabels(Repository repo, IEnumerable<Label> matchingLabels)
	{
		foreach (var label in matchingLabels)
		{
			await _gitHub.EditLabel(repo, label);
		}
	}

	private async Task DeleteLabels(Repository repo, IEnumerable<Label> oldLabels)
	{
		foreach (var label in oldLabels)
		{
			await _gitHub.DeleteLabel(repo, label);
		}
	}

	private static bool Matching(Label accountLabel, Label repoLabel)
		=> string.Equals(repoLabel.Name, accountLabel.Name, StringComparison.InvariantCultureIgnoreCase)
		   && string.Equals(repoLabel.Description, accountLabel.Description, StringComparison.InvariantCultureIgnoreCase)
		   && string.Equals(repoLabel.Color, accountLabel.Color, StringComparison.InvariantCultureIgnoreCase);

	private static bool NeedsUpdating(Label accountLabel, Label repoLabel)
		=> string.Equals(accountLabel.Name, repoLabel.Name, StringComparison.InvariantCultureIgnoreCase) &&
		   (!string.Equals(accountLabel.Description, repoLabel.Description, StringComparison.InvariantCultureIgnoreCase)
			|| !string.Equals(accountLabel.Color, repoLabel.Color, StringComparison.InvariantCultureIgnoreCase));
}