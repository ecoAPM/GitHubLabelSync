namespace GitHubLabelSync.Tests.Stubs;

public class Repository : Octokit.Repository
{
	public Repository(string name, bool archived = false)
	{
		Name = name;
		Archived = archived;
	}
}