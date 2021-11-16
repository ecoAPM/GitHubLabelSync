using Xunit;

namespace GitHubLabelSync.Tests;

public class FactoryTests
{
	[Fact]
	public void CanCreateApp()
	{
		//act
		var app = Factory.App("abc123", _ => { }, _ => { });

		//assert
		Assert.IsType<App>(app);
	}
}