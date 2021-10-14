namespace GitHubLabelSync.Tests.Stubs
{
	internal class Label : Octokit.Label
	{
		public Label(string name, string description, string color)
		{
			Name = name;
			Description = description;
			Color = color;
		}
	}
}