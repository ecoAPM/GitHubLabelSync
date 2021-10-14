# GitHub Label Sync

Synchronize GitHub issue labels across repositories

[![NuGet version](https://img.shields.io/nuget/v/GitHubLabelSync?logo=nuget&label=Install)](https://nuget.org/packages/GitHubLabelSync)
[![CI](https://github.com/ecoAPM/GitHubLabelSync/actions/workflows/CI.yml/badge.svg)](https://github.com/ecoAPM/GitHubLabelSync/actions/workflows/CI.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_GitHubLabelSync&metric=coverage)](https://sonarcloud.io/dashboard?id=ecoAPM_GitHubLabelSync)

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_GitHubLabelSync&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=ecoAPM_GitHubLabelSync)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_GitHubLabelSync&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=ecoAPM_GitHubLabelSync)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_GitHubLabelSync&metric=security_rating)](https://sonarcloud.io/dashboard?id=ecoAPM_GitHubLabelSync)

## Requirements

- .NET SDK 5.0

## Installation

```bash
~$ dotnet tool install -g GitHubLabelSync
```

## Setup

- Generate a Personal Access Token in GitHub from https://github.com/settings/tokens/new with the following access:
  - `repo`
  - `delete_repo`
- keep this key/token in a safe and private place, as you'll need it to sync your labels

### Wait, why do I need to give `delete_repo` rights? That sounds scary!

Good question!

The GitHub API does not provide direct access to default labels, and so they are obtained by creating (and immediately deleting) a temporary private repository.

You can see the code that performs these actions [here](https://github.com/ecoAPM/GitHubLabelSync/blob/main/src/Synchronizer.cs#L49-L53).

## Usage

```bash
~$ sync-labels <org/username> [options]
```

This tool is designed for GitHub organization administrators, as organizations have the ability to set the default labels for new repositories.

Individual users are limited to GitHub's default set of issue labels, but this tool can still be used to ensure all of your repositories have the latest set.

### Options

- `-k` or `--api-key`: (*required*) GitHub API Key (Personal Access Token)
- `-a` or `--no-add`: do not add new labels
- `-e` or `--no-edit`: do not edit existing labels
- `-d` or `--no-delete`: do not delete stale labels
- `-r` or `--dry-run`: do not perform any actions (equivalent to `-a -e -d`)
- `-h` or `--help`: view this information from the command line

## Contributing

Please be sure to read and follow ecoAPM's [Contribution Guidelines](CONTRIBUTING.md) when submitting issues or pull requests.