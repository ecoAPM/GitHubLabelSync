using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Octokit;
using Spectre.Console;
using Xunit;

namespace GitHubLabelSync.Tests
{
	public class AppTests
	{
		private static readonly Action<string> NoOp = _ => { };

		[Fact]
		public async Task SyncsAllReposFound()
		{
			//arrange
			var sync = Substitute.For<ISynchronizer>();
			sync.ValidateAccess().Returns(ValidationResult.Success());
			sync.GetRepositories(Arg.Any<Account>()).Returns(new[] { new Repository(), new Repository(), new Repository() });
			var app = new App(sync, NoOp, NoOp);

			var settings = new Settings { Name = "ecoAPM" };

			//act
			await app.Run(settings);

			//assert
			await sync.Received().GetAccount("ecoAPM");
			await sync.Received(3).SyncRepo(Arg.Any<Repository>(), settings, Arg.Any<IReadOnlyList<Label>>());
		}

		[Fact]
		public async Task ThrowsOnValidationFailure()
		{
			//arrange
			var sync = Substitute.For<ISynchronizer>();
			sync.ValidateAccess().Returns(ValidationResult.Error(":("));
			var app = new App(sync, NoOp, NoOp);

			var settings = new Settings { Name = "ecoAPM" };

			//act
			var task = app.Run(settings);

			//assert
			await Assert.ThrowsAnyAsync<Exception>(async () => await task);
		}
	}
}