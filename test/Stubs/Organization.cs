using Octokit;

namespace GitHubLabelSync.Tests.Stubs
{
	internal class Organization : Octokit.Organization
	{
		public Organization(string name)
		{
			Type = AccountType.Organization;
			Login = name;
		}
	}
}