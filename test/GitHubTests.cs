using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Octokit;
using Xunit;

namespace GitHubLabelSync.Tests;

public class GitHubTests
{
	private readonly Action<string> _noop = _ => { };

	[Fact]
	public async Task CanGetAccess()
	{
		//arrange
		var headers = new Dictionary<string, string>
			{
				{"X-OAuth-Scopes", "repo, delete_repo"}
			};

		var http = Substitute.For<IResponse>();
		http.Headers.Returns(headers);

		var response = Substitute.For<IApiResponse<string>>();
		response.HttpResponse.Returns(http);

		var client = Substitute.For<IGitHubClient>();
		client.Connection.Get<string>(Arg.Any<Uri>(), null, null).Returns(response);
		client.Connection.BaseAddress.Returns(new Uri("http://localhost/"));
		var gitHub = new GitHub(client, _noop, _noop);

		//act
		var access = await gitHub.GetAccess();

		//assert
		Assert.Equal(2, access.Count);
		Assert.Contains("repo", access);
		Assert.Contains("delete_repo", access);
	}

	[Fact]
	public async Task CanGetOrganization()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		client.Organization.Get("ecoAPM").Returns(new Stubs.Organization("ecoAPM"));
		var gitHub = new GitHub(client, _noop, _noop);

		//act
		var account = await gitHub.GetOrganization("ecoAPM");

		//assert
		Assert.Equal("ecoAPM", account.Login);
	}

	[Fact]
	public async Task CanGetUser()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		client.User.Get("SteveDesmond-ca").Returns(new Stubs.User("SteveDesmond-ca"));
		var gitHub = new GitHub(client, _noop, _noop);

		//act
		var account = await gitHub.GetUser("SteveDesmond-ca");

		//assert
		Assert.Equal("SteveDesmond-ca", account.Login);
	}

	[Fact]
	public async Task CanGetCurrentUser()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		client.User.Current().Returns(new Stubs.User("SteveDesmond-ca"));
		var gitHub = new GitHub(client, _noop, _noop);

		//act
		var account = await gitHub.GetCurrentUser();

		//assert
		Assert.Equal("SteveDesmond-ca", account.Login);
	}

	[Fact]
	public async Task CanGetRoleForUser()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		var membership = new OrganizationMembership(null, new StringEnum<MembershipState>(MembershipState.Active), new StringEnum<MembershipRole>(MembershipRole.Admin), null, null, null);
		client.Organization.Member.GetOrganizationMembership("ecoAPM", "SteveDesmond-ca").Returns(membership);
		var gitHub = new GitHub(client, _noop, _noop);

		//act
		var role = await gitHub.GetRole("SteveDesmond-ca", "ecoAPM");

		//assert
		Assert.Equal(MembershipRole.Admin, role);
	}

	[Fact]
	public async Task NonExistentOrgMemberReturnsNull()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		var membership = new OrganizationMembership(null, new StringEnum<MembershipState>(MembershipState.Active), new StringEnum<MembershipRole>(MembershipRole.Admin), null, null, null);
		client.Organization.Member.GetOrganizationMembership("ecoAPM", "SteveDesmond-ca").Throws(new NotFoundException(":(", HttpStatusCode.NotFound));
		var gitHub = new GitHub(client, _noop, _noop);

		//act
		var role = await gitHub.GetRole("SteveDesmond-ca", "ecoAPM");

		//assert
		Assert.Null(role);
	}

	[Fact]
	public async Task CanGetRepositoriesForOrganization()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		client.Repository.GetAllForOrg("ecoAPM").Returns(new[] { new Repository(), new Repository(), new Repository() });
		client.Repository.GetAllForUser("SteveDesmond-ca").Returns(new[] { new Repository(), new Repository() });
		var gitHub = new GitHub(client, _noop, _noop);

		var account = new Stubs.Organization("ecoAPM");

		//act
		var repos = await gitHub.GetRepositoriesForOrganization(account);

		//assert
		Assert.Equal(3, repos.Count);
	}

	[Fact]
	public async Task CanGetRepositoriesForUser()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		client.Repository.GetAllForOrg("ecoAPM").Returns(new[] { new Repository(), new Repository(), new Repository() });
		client.Repository.GetAllForUser("SteveDesmond-ca").Returns(new[] { new Repository(), new Repository() });
		var gitHub = new GitHub(client, _noop, _noop);

		var account = new Stubs.User("SteveDesmond-ca");
		//act

		var repos = await gitHub.GetRepositoriesForUser(account);

		//assert
		Assert.Equal(2, repos.Count);
	}

	[Fact]
	public async Task CanCreateTempRepoForOrganization()
	{
		//arrange
		var repo = new NewRepository("x");
		var client = Substitute.For<IGitHubClient>();
		var gitHub = new GitHub(client, _noop, _noop);

		var account = new Stubs.Organization("ecoAPM");
		await client.Repository.Create("ecoAPM", Arg.Do<NewRepository>(r => repo = r));

		//act
		await gitHub.CreateTempRepoForOrganization(account, "UnitTest");

		//assert
		Assert.Equal("UnitTest", repo.Name);
	}

	[Fact]
	public async Task CanCreateTempRepoForUser()
	{
		//arrange
		var repo = new NewRepository("x");
		var client = Substitute.For<IGitHubClient>();
		var gitHub = new GitHub(client, _noop, _noop);

		var account = new Stubs.User("SteveDesmond-ca");
		await client.Repository.Create(Arg.Do<NewRepository>(r => repo = r));

		//act
		await gitHub.CreateTempRepoForUser(account, "UnitTest");

		//assert
		Assert.Equal("UnitTest", repo.Name);
	}

	[Fact]
	public async Task CanDeleteTempRepoForOrganization()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		var gitHub = new GitHub(client, _noop, _noop);

		var account = new Stubs.Organization("ecoAPM");

		//act
		await gitHub.DeleteTempRepo(account, "UnitTest");

		//assert
		await client.Repository.Received().Delete("ecoAPM", "UnitTest");
	}

	[Fact]
	public async Task CanDeleteTempRepoForUser()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		var gitHub = new GitHub(client, _noop, _noop);

		var account = new Stubs.User("SteveDesmond-ca");

		//act
		await gitHub.DeleteTempRepo(account, "UnitTest");

		//assert
		await client.Repository.Received().Delete("SteveDesmond-ca", "UnitTest");
	}

	[Fact]
	public async Task CanGetRepoLabels()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		client.Issue.Labels.GetAllForRepository(123).Returns(new[] { new Label(), new Label(), new Label() });
		var gitHub = new GitHub(client, _noop, _noop);

		var repo = new Repository(123);

		//act
		var labels = await gitHub.GetLabels(repo);

		//assert
		Assert.Equal(3, labels.Count);
	}

	[Fact]
	public async Task CanAddLabel()
	{
		//arrange
		var newLabel = new NewLabel("x", "000000");
		var client = Substitute.For<IGitHubClient>();
		var gitHub = new GitHub(client, _noop, _noop);

		var repo = new Repository(123);
		var label = new Stubs.Label("test", "label for unit test", "538d43");
		await client.Issue.Labels.Create(123, Arg.Do<NewLabel>(l => newLabel = l));

		//act
		await gitHub.AddLabel(repo, label);

		//assert
		Assert.Equal("test", newLabel.Name);
		Assert.Equal("label for unit test", newLabel.Description);
		Assert.Equal("538d43", newLabel.Color);
	}

	[Fact]
	public async Task CanEditLabel()
	{
		//arrange
		var labelUpdate = new LabelUpdate("x", "000000");
		var client = Substitute.For<IGitHubClient>();
		var gitHub = new GitHub(client, _noop, _noop);

		var repo = new Repository(123);
		var label = new Stubs.Label("test", "label for unit test", "538d43");
		await client.Issue.Labels.Update(123, "test", Arg.Do<LabelUpdate>(l => labelUpdate = l));

		//act
		await gitHub.EditLabel(repo, label);

		//assert
		Assert.Equal("test", labelUpdate.Name);
		Assert.Equal("label for unit test", labelUpdate.Description);
		Assert.Equal("538d43", labelUpdate.Color);
	}

	[Fact]
	public async Task CanDeleteLabel()
	{
		//arrange
		var client = Substitute.For<IGitHubClient>();
		var gitHub = new GitHub(client, _noop, _noop);

		var repo = new Repository(123);
		var label = new Stubs.Label("test", "label for unit test", "538d43");

		//act
		await gitHub.DeleteLabel(repo, label);

		//assert
		await client.Issue.Labels.Received().Delete(123, "test");
	}
}
