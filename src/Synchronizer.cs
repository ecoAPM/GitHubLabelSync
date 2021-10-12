using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace GitHubLabelSync
{
    public class Synchronizer : ISynchronizer
    {
        private readonly IGitHub _gitHub;
        private readonly Random _random;
        private readonly Action<string> _setStatus;
        private readonly Action<string> _log;

        public Synchronizer(IGitHub gitHub, Random random, Action<string> setStatus, Action<string> log)
        {
            _gitHub = gitHub;
            _random = random;
            _setStatus = setStatus;
            _log = log;
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
                _log($"{name} is not an organization");
                return await _gitHub.GetUser(name);
            }
        }

        public async Task<IEnumerable<Repository>> GetRepositories(Account account)
            => account.Type == AccountType.Organization
                ? await _gitHub.GetRepositoriesForOrganization(account)
                : await _gitHub.GetRepositoriesForUser(account);

        public async Task<IReadOnlyList<Label>> GetAccountLabels(Account account)
        {
            _setStatus($"Finding labels for {account.Login}...");

            var repoName = $"label-sync-{_random.Next()}";
            var repo = account.Type == AccountType.Organization
                ? await _gitHub.CreateTempRepoForOrganization(account, repoName)
                : await _gitHub.CreateTempRepoForUser(account, repoName);
            var labels = await _gitHub.GetLabels(repo);
            await _gitHub.DeleteTempRepo(account, repoName);

            _log($"{labels.Count} labels for {account.Login}: {LabelNames(labels)}");
            return labels;
        }

        private static string LabelNames(IEnumerable<Label> labels)
            => string.Join(", ", labels.Select(l => l.Name));

        public async Task SyncRepo(Repository repo, Settings settings, IReadOnlyList<Label> accountLabels)
        {
            _log(repo.Name);
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
            _log($"{matchingLabels.Length,2} sync'd   : {LabelNames(matchingLabels)}");
        }

        private IEnumerable<Label> GetLabelsToAdd(Repository repo, IEnumerable<Label> accountLabels, IEnumerable<Label> repoLabels)
        {
            _setStatus($"Finding labels to add to {repo.Name}...");
            var newLabels = accountLabels.Where(al => repoLabels.All(rl => rl.Name != al.Name && rl.Description != al.Description)).ToArray();
            _log($"{newLabels.Length,2} to add   : {LabelNames(newLabels)}");
            return newLabels;
        }

        private IEnumerable<Label> GetLabelsToEdit(Repository repo, IEnumerable<Label> accountLabels, IEnumerable<Label> repoLabels)
        {
            _setStatus($"Finding labels to edit in {repo.Name}...");
            var editLabels = accountLabels.Where(al => repoLabels.Any(rl => NeedsUpdating(al, rl))).ToArray();
            _log($"{editLabels.Length,2} to edit  : {LabelNames(editLabels)}");
            return editLabels;
        }

        private IEnumerable<Label> GetLabelsToDelete(Repository repo, IEnumerable<Label> accountLabels, IEnumerable<Label> repoLabels)
        {
            _setStatus($"Finding labels to delete from {repo.Name}...");
            var oldLabels = repoLabels.Where(rl => accountLabels.All(al => al.Name != rl.Name && al.Description != rl.Description)).ToArray();
            _log($"{oldLabels.Length,2} to delete: {LabelNames(oldLabels)}");
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
            => repoLabel.Name == accountLabel.Name
               && repoLabel.Description == accountLabel.Description
               && repoLabel.Color == accountLabel.Color;

        private static bool NeedsUpdating(Label accountLabel, Label repoLabel)
            => accountLabel.Name == repoLabel.Name &&
               (accountLabel.Description != repoLabel.Description || accountLabel.Color != repoLabel.Color);
    }
}