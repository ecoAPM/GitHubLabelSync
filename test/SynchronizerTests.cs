using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Octokit;
using Xunit;

namespace GitHubLabelSync.Tests
{
	public class SynchronizerTests
	{
		private readonly Stubs.Label[] _accountLabels =
		{
			new("t1", "test1", "000000"),
			new("t2", "test2", "000000"),
			new("t3", "test3", "000000"),
			new("t4", "test4", "000000"),
			new("t5", "test5", "000000"),
			new("t6", "test6", "000000")
		};

		private readonly Stubs.Label[] _repoLabels =
		{
			new("t3", "", "000000"),
			new("t4", "test4", "FFFFFF"),
			new("t5", "", "FFFFFF"),
			new("t6", "test6", "000000"),
			new("t7", "test7", "000000"),
			new("t8", "test8", "000000"),
			new("t9", "test9", "000000"),
			new("t0", "test0", "000000")
		};

		private readonly Action<string> _noOp = _ => { };

		[Fact]
		public async Task ValidAccessReturnsSuccess()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetAccess().Returns(new[] { "repo", "delete_repo" });

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateAccess();

			//assert
			Assert.True(validation.Successful);
		}

		[Fact]
		public async Task ValidatingNoPrivateAccessFails()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetAccess().Returns(new[] { "public_repo", "delete_repo" });

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateAccess();

			//assert
			Assert.False(validation.Successful);
		}

		[Fact]
		public async Task ValidatingNoDeleteAccessFails()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetAccess().Returns(new[] { "repo" });

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateAccess();

			//assert
			Assert.False(validation.Successful);
		}

		[Fact]
		public async Task ValidOrgAdminReturnsSuccess()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetCurrentUser().Returns(new Stubs.User("SteveDesmond-ca"));
			gitHub.GetRole("SteveDesmond-ca", "ecoAPM").Returns(MembershipRole.Admin);

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateUser(new Stubs.Organization("ecoAPM"));

			//assert
			Assert.True(validation.Successful);
		}

		[Fact]
		public async Task NonOrgAdminFails()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetCurrentUser().Returns(new Stubs.User("SteveDesmond-ca"));
			gitHub.GetRole("SteveDesmond-ca", "ecoAPM").Returns(MembershipRole.Member);

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateUser(new Stubs.Organization("ecoAPM"));

			//assert
			Assert.False(validation.Successful);
		}

		[Fact]
		public async Task NonOrgMemberFails()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetCurrentUser().Returns(new Stubs.User("SteveDesmond-ca"));
			gitHub.GetRole("SteveDesmond-ca", "ecoAPM").Returns((MembershipRole?)null);

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateUser(new Stubs.Organization("ecoAPM"));

			//assert
			Assert.False(validation.Successful);
		}

		[Fact]
		public async Task SameUserAsTargetReturnsSuccess()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetCurrentUser().Returns(new Stubs.User("SteveDesmond-ca"));
			gitHub.GetRole("SteveDesmond-ca", "ecoAPM").Returns(MembershipRole.Admin);

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateUser(new Stubs.Organization("ecoAPM"));

			//assert
			Assert.True(validation.Successful);
		}

		[Fact]
		public async Task DifferentTargetUserFails()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetCurrentUser().Returns(new Stubs.User("SteveDesmond-ca"));
			gitHub.GetRole("SteveDesmond-ca", "ecoAPM").Returns(MembershipRole.Member);

			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var validation = await sync.ValidateUser(new Stubs.User("Unit-Test"));

			//assert
			Assert.False(validation.Successful);
		}

		[Fact]
		public async Task GetsOrganizationByDefault()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetOrganization("ecoAPM").Returns(new Organization());
			gitHub.GetUser("ecoAPM").Throws<Exception>();
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var account = await sync.GetAccount("ecoAPM");

			//assert
			Assert.IsType<Organization>(account);
		}

		[Fact]
		public async Task GetsUserIfOrganizationDoesNotExist()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetOrganization("SteveDesmond-ca").Throws<Exception>();
			gitHub.GetUser("SteveDesmond-ca").Returns(new User());
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			var account = await sync.GetAccount("SteveDesmond-ca");

			//assert
			Assert.IsType<User>(account);
		}

		[Fact]
		public async Task GetsRepositoriesForOrganizationBasedOnAccountType()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var account = new Stubs.Organization("ecoAPM");

			//act
			await sync.GetRepositories(account);

			//assert
			await gitHub.Received().GetRepositoriesForOrganization(account);
		}

		[Fact]
		public async Task GetsRepositoriesForUserBasedOnAccountType()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var account = new Stubs.User("SteveDesmond-ca");

			//act
			await sync.GetRepositories(account);

			//assert
			await gitHub.Received().GetRepositoriesForUser(account);
		}

		[Fact]
		public async Task GettingAccountLabelsCreatesTempRepoForOrganization()
		{
			//arrange
			var tempRepoName = "";
			var gitHub = Substitute.For<IGitHub>();
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var account = new Stubs.Organization("ecoAPM");
			await gitHub.CreateTempRepoForOrganization(account, Arg.Do<string>(name => tempRepoName = name));

			//act
			await sync.GetAccountLabels(account);

			//assert
			Assert.StartsWith("temp-label-sync-20", tempRepoName);
		}

		[Fact]
		public async Task GettingAccountLabelsCreatesTempRepoForUser()
		{
			//arrange
			var tempRepoName = "";
			var gitHub = Substitute.For<IGitHub>();
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var account = new Stubs.User("SteveDesmond-ca");
			await gitHub.CreateTempRepoForUser(account, Arg.Do<string>(name => tempRepoName = name));

			//act
			await sync.GetAccountLabels(account);

			//assert
			Assert.StartsWith("temp-label-sync-20", tempRepoName);
		}

		[Fact]
		public async Task GettingAccountLabelsDeletesTempRepoForOrganization()
		{
			//arrange
			var tempRepoName = "";
			var gitHub = Substitute.For<IGitHub>();
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var account = new Stubs.Organization("ecoAPM");
			await gitHub.DeleteTempRepo(account, Arg.Do<string>(name => tempRepoName = name));

			//act
			await sync.GetAccountLabels(account);

			//assert
			Assert.StartsWith("temp-label-sync-20", tempRepoName);
		}

		[Fact]
		public async Task GettingAccountLabelsDeletesTempRepoForUser()
		{
			//arrange
			var tempRepoName = "";
			var gitHub = Substitute.For<IGitHub>();
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var account = new Stubs.User("SteveDesmond-ca");
			await gitHub.DeleteTempRepo(account, Arg.Do<string>(name => tempRepoName = name));

			//act
			await sync.GetAccountLabels(account);

			//assert
			Assert.StartsWith("temp-label-sync-20", tempRepoName);
		}

		[Fact]
		public async Task PerformsCorrectActions()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetLabels(Arg.Any<Repository>()).Returns(_repoLabels);
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			//act
			await sync.SyncRepo(new Repository(), new Settings(), _accountLabels);

			//assert
			await gitHub.Received(2).AddLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.Received(3).EditLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.Received(4).DeleteLabel(Arg.Any<Repository>(), Arg.Any<Label>());
		}

		[Fact]
		public async Task NoAddDoesNotAdd()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetLabels(Arg.Any<Repository>()).Returns(_repoLabels);
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var settings = new Settings { NoAdd = true };

			//act
			await sync.SyncRepo(new Repository(), settings, _accountLabels);

			//assert
			await gitHub.DidNotReceive().AddLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.Received(3).EditLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.Received(4).DeleteLabel(Arg.Any<Repository>(), Arg.Any<Label>());
		}

		[Fact]
		public async Task NoEditDoesNotEdit()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetLabels(Arg.Any<Repository>()).Returns(_repoLabels);
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var settings = new Settings { NoEdit = true };

			//act
			await sync.SyncRepo(new Repository(), settings, _accountLabels);

			//assert
			await gitHub.Received(2).AddLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.DidNotReceive().EditLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.Received(4).DeleteLabel(Arg.Any<Repository>(), Arg.Any<Label>());
		}

		[Fact]
		public async Task NoDeleteDoesNotDelete()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetLabels(Arg.Any<Repository>()).Returns(_repoLabels);
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var settings = new Settings { NoDelete = true };

			//act
			await sync.SyncRepo(new Repository(), settings, _accountLabels);

			//assert
			await gitHub.Received(2).AddLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.Received(3).EditLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.DidNotReceive().DeleteLabel(Arg.Any<Repository>(), Arg.Any<Label>());
		}

		[Fact]
		public async Task DryRunPerformsNoActions()
		{
			//arrange
			var gitHub = Substitute.For<IGitHub>();
			gitHub.GetLabels(Arg.Any<Repository>()).Returns(_repoLabels);
			var sync = new Synchronizer(gitHub, _noOp, _noOp);

			var settings = new Settings { DryRun = true };

			//act
			await sync.SyncRepo(new Repository(), settings, _accountLabels);

			//assert
			await gitHub.DidNotReceive().AddLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.DidNotReceive().EditLabel(Arg.Any<Repository>(), Arg.Any<Label>());
			await gitHub.DidNotReceive().DeleteLabel(Arg.Any<Repository>(), Arg.Any<Label>());
		}
	}
}