#load "./constants.cake"
// #load "./versioning.cake"
// #load "./packaging.cake"
// #load "./package-checks.cake"
// #load "./package-tests.cake"
// #load "./test-results.cake"
// #load "./test-reports.cake"

using System;

public class BuildParameters
{
	private ISetupContext _context;
	private BuildSystem _buildSystem;

	public static BuildParameters Create(ISetupContext context)
	{
		var parameters = new BuildParameters(context);
		parameters.Validate();

		return parameters;
	}

	private BuildParameters(ISetupContext context)
	{
		_context = context;
		_buildSystem = context.BuildSystem();

		Target = _context.TargetTask.Name;
		TasksToExecute = _context.TasksToExecute.Select(t => t.Name);

		ProjectDirectory = _context.Environment.WorkingDirectory.FullPath + "/";

		Configuration = _context.Argument("configuration", DEFAULT_CONFIGURATION);

		MyGetApiKey = _context.EnvironmentVariable(MYGET_API_KEY);
		NuGetApiKey = _context.EnvironmentVariable(NUGET_API_KEY);
		ChocolateyApiKey = _context.EnvironmentVariable(CHOCO_API_KEY);
		GitHubAccessToken = _context.EnvironmentVariable(GITHUB_ACCESS_TOKEN);

		// BuildVersion = new BuildVersion(context, this);
	}

	public string Target { get; }
	public IEnumerable<string> TasksToExecute { get; }

	public ICakeContext Context => _context;

	public string Configuration { get; }

	// public BuildVersion BuildVersion { get; }
	// public string PackageVersion => BuildVersion.PackageVersion;
	public string PackageVersion => DEFAULT_VERSION;
	// public string AssemblyVersion => BuildVersion.AssemblyVersion;
	// public string AssemblyFileVersion => BuildVersion.AssemblyFileVersion;
	// public string AssemblyInformationalVersion => BuildVersion.AssemblyInformationalVersion;

	// public int PackageTestLevel { get; }

	public bool IsLocalBuild => _buildSystem.IsLocalBuild;
	public bool IsRunningOnUnix => _context.IsRunningOnUnix();
	public bool IsRunningOnWindows => _context.IsRunningOnWindows();
	public bool IsRunningOnAppVeyor => _buildSystem.AppVeyor.IsRunningOnAppVeyor;

	public string ProjectDirectory { get; }
	public string OutputDirectory => ProjectDirectory + "bin/" + Configuration + "/";
	public string Net20OutputDirectory => OutputDirectory + "net20/";
	public string NetCore21OutputDirectory => OutputDirectory + "netcoreapp2.1/";
	public string PackageDirectory => ProjectDirectory + "output/";
	public string ToolsDirectory => ProjectDirectory + "tools/";
	public string NuGetInstallDirectory => ToolsDirectory + NUGET_ID + "/";
	public string ChocolateyInstallDirectory => ToolsDirectory + CHOCO_ID + "/";

	public string NuGetPackageName => NUGET_ID + "." + PackageVersion + ".nupkg";
	public string ChocolateyPackageName => CHOCO_ID + "." + PackageVersion + ".nupkg";

	public string NuGetPackage => PackageDirectory + NuGetPackageName;
	public string ChocolateyPackage => PackageDirectory + ChocolateyPackageName;

	public string MyGetPushUrl => MYGET_PUSH_URL;
	public string NuGetPushUrl => NUGET_PUSH_URL;
	public string ChocolateyPushUrl => CHOCO_PUSH_URL;

	public string MyGetApiKey { get; }
	public string NuGetApiKey { get; }
	public string ChocolateyApiKey { get; }
	public string GitHubAccessToken { get; }

	// public string BranchName => BuildVersion.BranchName;
	// public bool IsReleaseBranch => BuildVersion.IsReleaseBranch;

	// public bool IsPreRelease => BuildVersion.IsPreRelease;
	// public bool ShouldPublishToMyGet =>
	// 	!IsPreRelease || LABELS_WE_PUBLISH_ON_MYGET.Contains(BuildVersion.PreReleaseLabel);
	// public bool ShouldPublishToNuGet =>
	// 	!IsPreRelease || LABELS_WE_PUBLISH_ON_NUGET.Contains(BuildVersion.PreReleaseLabel);
	// public bool ShouldPublishToChocolatey =>
	// 	!IsPreRelease || LABELS_WE_PUBLISH_ON_CHOCOLATEY.Contains(BuildVersion.PreReleaseLabel);
	// public bool IsProductionRelease =>
	// 	!IsPreRelease || LABELS_WE_RELEASE_ON_GITHUB.Contains(BuildVersion.PreReleaseLabel);

	public string GetPathToConsoleRunner(string version)
	{
	 	return ToolsDirectory + "NUnit.ConsoleRunner." + version + "/tools/nunit3-console.exe";
	}

	private void Validate()
	{
		var validationErrors = new List<string>();

	// 	if (TasksToExecute.Contains("PublishPackages"))
	// 	{
	// 		if (ShouldPublishToMyGet && string.IsNullOrEmpty(MyGetApiKey))
	// 			validationErrors.Add("MyGet ApiKey was not set.");
	// 		if (ShouldPublishToNuGet && string.IsNullOrEmpty(NuGetApiKey))
	// 			validationErrors.Add("NuGet ApiKey was not set.");
	// 		if (ShouldPublishToChocolatey && string.IsNullOrEmpty(ChocolateyApiKey))
	// 			validationErrors.Add("Chocolatey ApiKey was not set.");
	// 	}

	// 	if (TasksToExecute.Contains("CreateDraftRelease") && (IsReleaseBranch || IsProductionRelease))
	// 	{
	// 		if (string.IsNullOrEmpty(GitHubAccessToken))
	// 			validationErrors.Add("GitHub Access Token was not set.");
	// 	}

		if (validationErrors.Count > 0)
	 	{
	 		DumpSettings();

	 		var msg = new StringBuilder("Parameter validation failed! See settings above.\n\nErrors found:\n");
	 		foreach (var error in validationErrors)
	 			msg.AppendLine("  " + error);

	 		throw new InvalidOperationException(msg.ToString());
	 	}
	}

	public void DumpSettings()
	{
	 	Console.WriteLine("\nTASKS");
	 	Console.WriteLine("Target:                       " + Target);
	 	Console.WriteLine("TasksToExecute:               " + string.Join(", ", TasksToExecute));

	 	Console.WriteLine("\nENVIRONMENT");
	 	Console.WriteLine("IsLocalBuild:                 " + IsLocalBuild);
	 	Console.WriteLine("IsRunningOnWindows:           " + IsRunningOnWindows);
	 	Console.WriteLine("IsRunningOnUnix:              " + IsRunningOnUnix);
	 	Console.WriteLine("IsRunningOnAppVeyor:          " + IsRunningOnAppVeyor);

	 	Console.WriteLine("\nVERSIONING");
	 	Console.WriteLine("PackageVersion:               " + PackageVersion);
	// 	Console.WriteLine("AssemblyVersion:              " + AssemblyVersion);
	// 	Console.WriteLine("AssemblyFileVersion:          " + AssemblyFileVersion);
	// 	Console.WriteLine("AssemblyInformationalVersion: " + AssemblyInformationalVersion);
	// 	Console.WriteLine("SemVer:                       " + BuildVersion.SemVer);
	// 	Console.WriteLine("IsPreRelease:                 " + BuildVersion.IsPreRelease);
	// 	Console.WriteLine("PreReleaseLabel:              " + BuildVersion.PreReleaseLabel);
	// 	Console.WriteLine("PreReleaseSuffix:             " + BuildVersion.PreReleaseSuffix);

	 	Console.WriteLine("\nDIRECTORIES");
	 	Console.WriteLine("Project:   " + ProjectDirectory);
	 	Console.WriteLine("Output:    " + OutputDirectory);
	 	//Console.WriteLine("Source:    " + SourceDirectory);
	 	//Console.WriteLine("NuGet:     " + NuGetDirectory);
	 	//Console.WriteLine("Choco:     " + ChocoDirectory);
	 	Console.WriteLine("Package:   " + PackageDirectory);
	 	//Console.WriteLine("ZipImage:  " + ZipImageDirectory);
	 	//Console.WriteLine("ZipTest:   " + ZipTestDirectory);
	 	//Console.WriteLine("NuGetTest: " + NuGetTestDirectory);
	 	//Console.WriteLine("ChocoTest: " + ChocolateyTestDirectory);

	 	Console.WriteLine("\nBUILD");
	 	//Console.WriteLine("Build With:      " + (UsingXBuild ? "XBuild" : "MSBuild"));
	 	Console.WriteLine("Configuration:   " + Configuration);

	 	Console.WriteLine("\nPACKAGING");
	 	Console.WriteLine("MyGetPushUrl:              " + MyGetPushUrl);
	 	Console.WriteLine("NuGetPushUrl:              " + NuGetPushUrl);
	 	Console.WriteLine("ChocolateyPushUrl:         " + ChocolateyPushUrl);
	 	Console.WriteLine("MyGetApiKey:               " + (!string.IsNullOrEmpty(MyGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
	 	Console.WriteLine("NuGetApiKey:               " + (!string.IsNullOrEmpty(NuGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
	 	Console.WriteLine("ChocolateyApiKey:          " + (!string.IsNullOrEmpty(ChocolateyApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));

	// 	Console.WriteLine("\nPUBLISHING");
	// 	Console.WriteLine("ShouldPublishToMyGet:      " + ShouldPublishToMyGet);
	// 	Console.WriteLine("ShouldPublishToNuGet:      " + ShouldPublishToNuGet);
	// 	Console.WriteLine("ShouldPublishToChocolatey: " + ShouldPublishToChocolatey);

	// 	Console.WriteLine("\nRELEASING");
	// 	Console.WriteLine("BranchName:                   " + BranchName);
	// 	Console.WriteLine("IsReleaseBranch:              " + IsReleaseBranch);
	// 	Console.WriteLine("IsProductionRelease:          " + IsProductionRelease);
	}
}