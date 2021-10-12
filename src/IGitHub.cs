﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLabelSync
{
    public interface IGitHub
    {
        Task<Account> GetOrganization(string name);
        Task<Account> GetUser(string name);
        
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