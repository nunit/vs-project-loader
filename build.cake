#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.1

//////////////////////////////////////////////////////////////////////
// PROJECT-SPECIFIC
//////////////////////////////////////////////////////////////////////
var SOLUTION_FILE = "vs-project-loader.sln";
var NUSPEC_FILE = "vs-project-loader.nuspec";
var UNIT_TEST_ASSEMBLY = "vs-project-loader.tests.dll";

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

//////////////////////////////////////////////////////////////////////
// SET PACKAGE VERSION
//////////////////////////////////////////////////////////////////////

var version = "3.5.0";

var dbgSuffix = configuration == "Debug" ? "-dbg" : "";
var packageVersion = version + dbgSuffix;

if (BuildSystem.IsRunningOnAppVeyor)
{
	var tag = AppVeyor.Environment.Repository.Tag;

	if (tag.IsTag)
	{
		packageVersion = tag.Name;
	}
	else
	{
		var buildNumber = AppVeyor.Environment.Build.Number.ToString("00000");
		var branch = AppVeyor.Environment.Repository.Branch;
		var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

		if (branch == "master" && !isPullRequest)
		{
			packageVersion = version + "-dev-" + buildNumber + dbgSuffix;
		}
		else
		{
			var suffix = "-ci-" + buildNumber + dbgSuffix;

			if (isPullRequest)
				suffix += "-pr-" + AppVeyor.Environment.PullRequest.Number;
			else
				suffix += "-" + branch;

			// Nuget limits "special version part" to 20 chars. Add one for the hyphen.
			if (suffix.Length > 21)
				suffix = suffix.Substring(0, 21);

                        suffix = suffix.Replace(".", "");

			packageVersion = version + suffix;
		}
	}

	AppVeyor.UpdateBuildVersion(packageVersion);
}

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var PACKAGE_DIR = PROJECT_DIR + "package/";
var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";

// File PATHS
var SOLUTION_PATH = PROJECT_DIR + SOLUTION_FILE;
var NUSPEC_PATH = PROJECT_DIR + NUSPEC_FILE;
var UNIT_TEST_PATH = BIN_DIR + UNIT_TEST_ASSEMBLY;

// Package sources for nuget restore
var PACKAGE_SOURCE = new string[]
	{
		"https://www.nuget.org/api/v2",
		"https://www.myget.org/F/nunit/api/v2"
	};

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(BIN_DIR);
});


//////////////////////////////////////////////////////////////////////
// INITIALIZE FOR BUILD
//////////////////////////////////////////////////////////////////////

Task("NuGetRestore")
    .Does(() =>
{
    NuGetRestore(SOLUTION_PATH, new NuGetRestoreSettings()
	{
		Source = PACKAGE_SOURCE
	});
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("NuGetRestore")
    .Does(() =>
    {
        DotNetBuild(SOLUTION_PATH, settings => settings
            .WithTarget("Build")
            .SetConfiguration(configuration)
            .SetVerbosity(Verbosity.Minimal)
        );
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
	{
		NUnit3(UNIT_TEST_PATH);
	});

//////////////////////////////////////////////////////////////////////
// PACKAGE
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.Does(() => 
	{
		CreateDirectory(PACKAGE_DIR);

        NuGetPack(NUSPEC_PATH, new NuGetPackSettings()
        {
            Version = packageVersion,
            BasePath = BIN_DIR,
            OutputDirectory = PACKAGE_DIR
        });
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Rebuild")
    .IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("Appveyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

Task("Travis")
	.IsDependentOn("Build")
	.IsDependentOn("Test");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
