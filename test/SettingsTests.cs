using Xunit;

namespace GitHubLabelSync.Tests;

public class SettingsTests
{
	[Fact]
	public void PositiveFlagsSetByDefault()
	{
		//act
		var settings = new Settings();

		//assert
		Assert.True(settings.Add);
		Assert.True(settings.Edit);
		Assert.True(settings.Delete);
	}

	[Fact]
	public void DryRunSetsAllFlags()
	{
		//act
		var settings = new Settings { DryRun = true };

		//assert
		Assert.True(settings.NoAdd);
		Assert.True(settings.NoEdit);
		Assert.True(settings.NoDelete);
	}

	[Fact]
	public void AddIsNegativeNoAdd()
	{
		//act
		var settings = new Settings { NoAdd = true };

		//assert
		Assert.False(settings.Add);
	}

	[Fact]
	public void EditIsNegativeNoEdit()
	{
		//act
		var settings = new Settings { NoEdit = true };

		//assert
		Assert.False(settings.Edit);
	}

	[Fact]
	public void DeleteIsNegativeNoDelete()
	{
		//act
		var settings = new Settings { NoDelete = true };

		//assert
		Assert.False(settings.Delete);
	}

	[Fact]
	public void APIKeyIsRequired()
	{
		//arrange
		var settings = new Settings();

		//act
		var valid = settings.Validate();

		//assert
		Assert.False(valid.Successful);
	}
}