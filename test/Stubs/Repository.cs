using Octokit;

namespace GitHubLabelSync.Tests.Stubs;

public class Repository : Octokit.Repository
{
	public Repository(string name, bool archived = false)
		: base(null, null, null, null, null, null, null, 0, null, null, name, null, false, null, null, null, false, false, 0, 0, null, 0, null, DateTimeOffset.Now, DateTimeOffset.Now, null, null, null, null, false, false, false, false, 0, 0, null, null, null, archived, 0, null, RepositoryVisibility.Public, Array.Empty<string>(), null, null)
	{
	}
}