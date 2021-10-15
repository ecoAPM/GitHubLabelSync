using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLabelSync
{
	public interface IGitHub
	{
		Task<IReadOnlyList<string>> GetAccess();
		Task<Account> GetOrganization(string name);
		Task<Account> GetCurrentUser();
		Task<Account> GetUser(string name);
		Task<MembershipRole?> GetRole(string user, string org);

		Task<IReadOnlyList<Repository>> GetRepositoriesForOrganization(Account account);
		Task<IReadOnlyList<Repository>> GetRepositoriesForUser(Account account);

		Task<Repository> CreateTempRepoForOrganization(Account account, string repoName);
		Task<Repository> CreateTempRepoForUser(Account account, string repoName);
		Task DeleteTempRepo(Account account, string repoName);

		Task<IReadOnlyList<Label>> GetLabels(Repository repo);
		Task AddLabel(Repository repo, Label label);
		Task EditLabel(Repository repo, Label label);
		Task DeleteLabel(Repository repo, Label label);
	}
}