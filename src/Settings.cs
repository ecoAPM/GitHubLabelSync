using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubLabelSync
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, "<org/username>")]
		[Description("The name of the GitHub organization or username to sync")]
		public string Name { get; init; }

		[CommandOption("-k|--api-key")]
		[Description("GitHub API Key (Personal Access Token)")]
		public string APIKey { get; init; }

		private readonly bool _noAdd;

		[CommandOption("-a|--no-add")]
		[Description("Do not add new labels")]
		public bool NoAdd
		{
			get => _noAdd || DryRun;
			init => _noAdd = value;
		}

		private readonly bool _noEdit;

		[CommandOption("-e|--no-edit")]
		[Description("Do not edit existing labels")]
		public bool NoEdit
		{
			get => _noEdit || DryRun;
			init => _noEdit = value;
		}

		private readonly bool _noDelete;

		[CommandOption("-d|--no-delete")]
		[Description("Do not delete existing labels")]
		public bool NoDelete
		{
			get => _noDelete || DryRun;
			init => _noDelete = value;
		}

		[CommandOption("-r|--dry-run")]
		[Description("Do not perform any actions (equivalent to `-a -e -d`)")]
		public bool DryRun { get; init; }

		public bool Add => !NoAdd;
		public bool Edit => !NoEdit;
		public bool Delete => !NoDelete;

		public override ValidationResult Validate()
			=> string.IsNullOrWhiteSpace(APIKey)
				? ValidationResult.Error("GitHub API Key (Personal Access Token) is required")
				: base.Validate();
	}
}