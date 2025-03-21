///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

BuildParameters.Tasks.ClearAppVeyorCacheTask = Task("Clear-AppVeyor-Cache")
    .Does(() =>
        RequireAddin(@"#addin nuget:?package=Cake.AppVeyor&version=3.0.0&loaddependencies=true
        AppVeyorClearCache(new AppVeyorSettings() { ApiToken = EnvironmentVariable(""TEMP_APPVEYOR_TOKEN"") },
            EnvironmentVariable(""TEMP_APPVEYOR_ACCOUNT_NAME""),
            EnvironmentVariable(""TEMP_APPVEYOR_PROJECT_SLUG""));
        ",
        new Dictionary<string, string> {{"TEMP_APPVEYOR_TOKEN", BuildParameters.AppVeyor.ApiToken},
            {"TEMP_APPVEYOR_ACCOUNT_NAME", BuildParameters.AppVeyorAccountName},
            {"TEMP_APPVEYOR_PROJECT_SLUG", BuildParameters.AppVeyorProjectSlug}}
));

///////////////////////////////////////////////////////////////////////////////
// BUILD PROVIDER
///////////////////////////////////////////////////////////////////////////////

public class AppVeyorTagInfo : ITagInfo
{
    public AppVeyorTagInfo(IAppVeyorProvider appVeyor)
    {
        IsTag = appVeyor.Environment.Repository.Tag.IsTag;
        Name = appVeyor.Environment.Repository.Tag.Name;
    }

    public bool IsTag { get; }

    public string Name { get; }
}

public class AppVeyorRepositoryInfo : IRepositoryInfo
{
    public AppVeyorRepositoryInfo(IAppVeyorProvider appVeyor)
    {
        Branch = appVeyor.Environment.Repository.Branch;
        Name = appVeyor.Environment.Repository.Name;
        Tag = new AppVeyorTagInfo(appVeyor);
    }

    public string Branch { get; }

    public string Name { get; }

    public ITagInfo Tag { get; }
}

public class AppVeyorPullRequestInfo : IPullRequestInfo
{
    public AppVeyorPullRequestInfo(IAppVeyorProvider appVeyor)
    {
        IsPullRequest = appVeyor.Environment.PullRequest.IsPullRequest;
    }

    public bool IsPullRequest { get; }
}

public class AppVeyorBuildInfo : IBuildInfo
{
    public AppVeyorBuildInfo(IAppVeyorProvider appVeyor)
    {
        Number = appVeyor.Environment.Build.Number.ToString();
    }

    public string Number { get; }
}

public class AppVeyorBuildProvider : IBuildProvider
{
    public AppVeyorBuildProvider(IAppVeyorProvider appVeyor, ICakeContext context)
    {
        Repository = new AppVeyorRepositoryInfo(appVeyor);
        PullRequest = new AppVeyorPullRequestInfo(appVeyor);
        Build = new AppVeyorBuildInfo(appVeyor);

        _appVeyor = appVeyor;
        _context = context;
    }

    public IRepositoryInfo Repository { get; }

    public IPullRequestInfo PullRequest { get; }

    public IBuildInfo Build { get; }

    public bool SupportsTokenlessCodecov { get; } = false;

    public BuildProviderType Type { get; } = BuildProviderType.AppVeyor;

    public IEnumerable<string> PrintVariables { get; } = new[] {
        "CI",
        "APPVEYOR_API_URL",
        "APPVEYOR_PROJECT_ID",
        "APPVEYOR_PROJECT_NAME",
        "APPVEYOR_PROJECT_SLUG",
        "APPVEYOR_BUILD_FOLDER",
        "APPVEYOR_BUILD_ID",
        "APPVEYOR_BUILD_NUMBER",
        "APPVEYOR_BUILD_VERSION",
        "APPVEYOR_PULL_REQUEST_NUMBER",
        "APPVEYOR_PULL_REQUEST_TITLE",
        "APPVEYOR_JOB_ID",
        "APPVEYOR_REPO_PROVIDER",
        "APPVEYOR_REPO_SCM",
        "APPVEYOR_REPO_NAME",
        "APPVEYOR_REPO_BRANCH",
        "APPVEYOR_REPO_TAG",
        "APPVEYOR_REPO_TAG_NAME",
        "APPVEYOR_REPO_COMMIT",
        "APPVEYOR_REPO_COMMIT_AUTHOR",
        "APPVEYOR_REPO_COMMIT_TIMESTAMP",
        "APPVEYOR_SCHEDULED_BUILD",
        "APPVEYOR_FORCED_BUILD",
        "APPVEYOR_RE_BUILD",
        "PLATFORM",
        "CONFIGURATION"
    };

    private readonly IAppVeyorProvider  _appVeyor;

    private readonly ICakeContext _context;

    public void UploadArtifact(FilePath file)
    {
        _context.Information("Uploading artifact from path: {0}", file.FullPath);
        _appVeyor.UploadArtifact(file.FullPath);
    }
}
