namespace GitHubLabelSync.Tests.Stubs;

internal class Label : Octokit.Label
{
	public Label(string name, string description, string color)
		: base(0, null, name, null, color, description, false)
	{
	}
}