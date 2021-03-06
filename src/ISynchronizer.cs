using Octokit;
using Spectre.Console;

namespace GitHubLabelSync;

public interface ISynchronizer
{
	Task<ValidationResult> ValidateAccess();
	Task<ValidationResult> ValidateUser(Account account);

	Task<Account> GetAccount(string name);
	Task<IReadOnlyList<Repository>> GetRepositories(Account account);
	Task<IReadOnlyList<Label>> GetAccountLabels(Account account);

	Task SyncRepo(Repository repo, Settings settings, IReadOnlyList<Label> accountLabels);
}