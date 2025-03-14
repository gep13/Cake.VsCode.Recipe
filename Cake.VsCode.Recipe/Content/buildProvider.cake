BuildParameters.Tasks.PrintCiProviderEnvironmentVariablesTask = Task("Print-CI-Provider-Environment-Variables")
    .Does(() =>
{
        var variables = BuildParameters.BuildProvider.PrintVariables ?? Enumerable.Empty<string>();
        if (!variables.Any())
        {
            Information("No environment variables is available for current provider.");
            return;
        }

        var maxlen = variables.Max(v => v.Length);

        foreach (var variable in variables.OrderBy(v => v.Length).ThenBy(v => v))
        {
            var padKey = variable.PadLeft(maxlen);
            Information("{0}: {1}", padKey, EnvironmentVariable(variable));
        }
});

public interface ITagInfo
{
    bool IsTag { get; }

    string Name { get; }
}

public interface IRepositoryInfo
{
    string Branch { get; }

    string Name { get; }

    ITagInfo Tag { get; }
}

public interface IPullRequestInfo
{
    bool IsPullRequest { get; }
}

public interface IBuildInfo
{
    string Number { get; }
}

public interface IBuildProvider
{
    IRepositoryInfo Repository { get; }

    IPullRequestInfo PullRequest { get; }

    IBuildInfo Build { get; }

    bool SupportsTokenlessCodecov { get; }

    IEnumerable<string> PrintVariables { get; }

    void UploadArtifact(FilePath file);

    BuildProviderType Type { get; }
}

public enum BuildProviderType
{
    TeamCity,
    GitHubActions,
    Local,
    AppVeyor
}

public static IBuildProvider GetBuildProvider(ICakeContext context, BuildSystem buildSystem)
{
    if (buildSystem.IsRunningOnTeamCity)
    {
        context.Information("Using TeamCity Provider...");
        return new TeamCityBuildProvider(buildSystem.TeamCity, context);
    }

    if (buildSystem.IsRunningOnGitHubActions)
    {
        context.Information("Using GitHub Action Provider...");
        return new GitHubActionBuildProvider(context);
    }

    if (buildSystem.IsRunningOnAppVeyor)
    {
        context.Information("Using AppVeyor Provider...");
        return new AppVeyorBuildProvider(buildSystem.AppVeyor, context);
    }

    // always fallback to Local Build
    context.Information("Using Local Build Provider...");
    return new LocalBuildBuildProvider(context);
}
