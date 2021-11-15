using Octokit;

namespace GitHubLabelSync.Tests.Stubs;

public class ArchivedRepository : Repository
{
	public ArchivedRepository()
	{
		Archived = true;
	}
}
