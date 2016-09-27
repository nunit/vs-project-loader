#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

//////////////////////////////////////////////////////////////////////
// SET PACKAGE VERSION
//////////////////////////////////////////////////////////////////////

var version = "3.5.0";
var modifier = "";

var dbgSuffix = configuration == "Debug" ? "-dbg" : "";
var packageVersion = version + modifier + dbgSuffix;

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

var packageName = "NUnitV2Driver-" + packageVersion;

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var PACKAGE_DIR = PROJECT_DIR + "package/";
var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";

// Files
var SOLUTION_FILE = PROJECT_DIR + "vs-project-loader.sln";
var UNIT_TESTS = BIN_DIR + "vs-project-loader.tests.dll";
//var INTEGRATION_TESTS = BIN_DIR + "v2-tests/v2-test-assembly.dll";

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
    NuGetRestore(SOLUTION_FILE);
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("NuGetRestore")
    .Does(() =>
    {
        DotNetBuild(SOLUTION_FILE, settings => settings
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
		NUnit3(UNIT_TESTS);
		//NUnit3(INTEGRATION_TESTS);
	});

//////////////////////////////////////////////////////////////////////
// PACKAGE
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.Does(() => 
	{
		CreateDirectory(PACKAGE_DIR);

        NuGetPack("nunit.v2.driver.nuspec", new NuGetPackSettings()
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

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
