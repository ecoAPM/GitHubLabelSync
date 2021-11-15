using System;
using System.Reflection;
using Octokit;

namespace GitHubLabelSync;

public static class Factory
{
	public static App App(string apiKey, Action<string> setStatus, Action<string> log)
	{
		var name = Assembly.GetExecutingAssembly().GetName().Name;
		var value = new ProductHeaderValue(name);
		var client = new GitHubClient(value) { Credentials = new Credentials(apiKey) };

		var gitHub = new GitHub(client, setStatus, log);
		var sync = new Synchronizer(gitHub, setStatus, log);
		return new App(sync, setStatus, log);
	}
}
