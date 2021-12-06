using System.Text;
using NSubstitute;
using Octokit;
using Spectre.Console;
using Xunit;
using Repository = GitHubLabelSync.Tests.Stubs.Repository;

namespace GitHubLabelSync.Tests;

public class AppTests
{
	private static readonly Action<string> NoOp = _ => { };

	[Fact]
	public async Task SyncsAllReposFound()
	{
		//arrange
		var sync = Substitute.For<ISynchronizer>();
		sync.ValidateAccess().Returns(ValidationResult.Success());
		sync.ValidateUser(Arg.Any<Account>()).Returns(ValidationResult.Success());
		sync.GetRepositories(Arg.Any<Account>()).Returns(new[] { new Repository("test1"), new Repository("test2"), new Repository("test3") });
		var app = new App(sync, NoOp, NoOp);

		var settings = new Settings { Name = "ecoAPM" };

		//act
		await app.Run(settings);

		//assert
		await sync.Received().GetAccount("ecoAPM");
		await sync.Received(3).SyncRepo(Arg.Any<Repository>(), settings, Arg.Any<IReadOnlyList<Label>>());
	}

	[Fact]
	public async Task SyncsFilteredRepos()
	{
		//arrange
		var sync = Substitute.For<ISynchronizer>();
		sync.ValidateAccess().Returns(ValidationResult.Success());
		sync.ValidateUser(Arg.Any<Account>()).Returns(ValidationResult.Success());
		sync.GetRepositories(Arg.Any<Account>()).Returns(new[]
		{
			new Repository("other1"),
			new Repository("test-abc1"),
			new Repository("test-abc2"),
			new Repository("def"),
			new Repository("other22"),
		});
		var app = new App(sync, NoOp, NoOp);

		var settings = new Settings
		{
			Name = "ecoAPM",
			Filters = new[] { "abc", "def" }
		};

		//act
		await app.Run(settings);

		//assert
		await sync.Received().GetAccount("ecoAPM");
		await sync.Received(3).SyncRepo(Arg.Any<Repository>(), settings, Arg.Any<IReadOnlyList<Label>>());
	}

	[Fact]
	public async Task SkipsArchivedRepos()
	{
		//arrange
		var sync = Substitute.For<ISynchronizer>();
		sync.ValidateAccess().Returns(ValidationResult.Success());
		sync.ValidateUser(Arg.Any<Account>()).Returns(ValidationResult.Success());
		sync.GetRepositories(Arg.Any<Account>()).Returns(new[]
		{
			new Repository("test1"),
			new Repository("test2", true),
			new Repository("test3")
		});
		var app = new App(sync, NoOp, NoOp);

		var settings = new Settings { Name = "ecoAPM" };

		//act
		await app.Run(settings);

		//assert
		await sync.Received(2).SyncRepo(Arg.Any<Repository>(), settings, Arg.Any<IReadOnlyList<Label>>());
	}

	[Fact]
	public async Task ShowsMessageWhenNoRepos()
	{
		//arrange
		var sync = Substitute.For<ISynchronizer>();
		sync.ValidateAccess().Returns(ValidationResult.Success());
		sync.ValidateUser(Arg.Any<Account>()).Returns(ValidationResult.Success());
		sync.GetRepositories(Arg.Any<Account>()).Returns(Array.Empty<Repository>());
		var output = new StringBuilder();
		var app = new App(sync, NoOp, s => output.AppendLine(s));

		var settings = new Settings { Name = "ecoAPM" };

		//act
		await app.Run(settings);

		//assert
		Assert.Contains("no repositories to sync", output.ToString());
	}

	[Fact]
	public async Task ThrowsOnAccessValidationFailure()
	{
		//arrange
		var sync = Substitute.For<ISynchronizer>();
		sync.ValidateAccess().Returns(ValidationResult.Error(":("));
		sync.ValidateUser(Arg.Any<Account>()).Returns(ValidationResult.Success());
		var app = new App(sync, NoOp, NoOp);

		var settings = new Settings { Name = "ecoAPM" };

		//act
		var task = app.Run(settings);

		//assert
		await Assert.ThrowsAnyAsync<Exception>(async () => await task);
	}

	[Fact]
	public async Task ThrowsOnUserValidationFailure()
	{
		//arrange
		var sync = Substitute.For<ISynchronizer>();
		sync.ValidateAccess().Returns(ValidationResult.Success());
		sync.ValidateUser(Arg.Any<Account>()).Returns(ValidationResult.Error(":("));
		var app = new App(sync, NoOp, NoOp);

		var settings = new Settings { Name = "ecoAPM" };

		//act
		var task = app.Run(settings);

		//assert
		await Assert.ThrowsAnyAsync<Exception>(async () => await task);
	}
}