using Octokit;

namespace GitHubLabelSync.Tests.Stubs;

internal class User : Octokit.User
{
	public User(string name)
	{
		Type = AccountType.User;
		Login = name;
	}
}
