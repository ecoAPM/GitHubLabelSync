using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLabelSync
{
    public interface ISynchronizer
    {
        Task<Account> GetAccount(string name);
        Task<IEnumerable<Repository>> GetRepositories(Account account);
        Task<IReadOnlyList<Label>> GetAccountLabels(Account account);
        Task SyncRepo(Repository repo, Settings settings, IReadOnlyList<Label> accountLabels);
    }
}