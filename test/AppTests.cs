using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Octokit;
using Xunit;

namespace GitHubLabelSync.Tests
{
	public class AppTests
	{
		[Fact]
		public async Task SyncsAllReposFound()
		{
			//arrange
			var sync = Substitute.For<ISynchronizer>();
			sync.GetRepositories(Arg.Any<Account>()).Returns(new[] { new Repository(), new Repository(), new Repository() });
			var app = new App(sync, _ => { }, _ => { });

			var settings = new Settings { Name = "ecoAPM" };

			//act
			await app.Run(settings);

			//assert
			await sync.Received().GetAccount("ecoAPM");
			await sync.Received(3).SyncRepo(Arg.Any<Repository>(), settings, Arg.Any<IReadOnlyList<Label>>());
		}
	}
}