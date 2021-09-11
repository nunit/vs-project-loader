//////////////////////////////////////////////////////////////////////
// PACKAGE METADATA
//////////////////////////////////////////////////////////////////////

var TITLE = "NUnit 3 - Visual Studio Project Loader Extension";
var AUTHORS = new [] { "Charlie Poole" };
var OWNERS = new [] { "Charlie Poole" };
var DESCRIPTION = "This extension allows NUnit to recognize and load solutions and projects in Visual Studio format. It supports files of type .sln, .csproj, .vbproj, .vjsproj, .vcproj and .fsproj.";
var SUMMARY = "NUnit Engine extension for loading Visual Studio formatted projects.";
var COPYRIGHT = "Copyright (c) 2018 Charlie Poole";
var RELEASE_NOTES = new [] { "See https://raw.githubusercontent.com/nunit/vs-project-loader/main/CHANGES.txt" };
var TAGS = new [] { "nunit", "test", "testing", "tdd", "runner" };
var PROJECT_URL = new Uri("http://nunit.org");
var ICON_URL = new Uri("https://cdn.rawgit.com/nunit/resources/master/images/icon/nunit_256.png");
var LICENSE_URL = new Uri("http://nunit.org/nuget/nunit3-license.txt");
var PROJECT_SOURCE_URL = new Uri( GITHUB_SITE );
var PACKAGE_SOURCE_URL = new Uri( GITHUB_SITE );
var BUG_TRACKER_URL = new Uri(GITHUB_SITE + "/issues");
var DOCS_URL = new Uri(WIKI_PAGE);
var MAILING_LIST_URL = new Uri("https://groups.google.com/forum/#!forum/nunit-discuss");

//////////////////////////////////////////////////////////////////////
// BUILD NUGET PACKAGE
//////////////////////////////////////////////////////////////////////

public void BuildNuGetPackage(BuildParameters parameters)
{
	NuGetPack(new NuGetPackSettings()
	{
		Id = NUGET_ID,
		Version = parameters.PackageVersion,
		Title = TITLE,
		Authors = AUTHORS,
		Owners = OWNERS,
		Description = DESCRIPTION,
		Summary = SUMMARY,
		ProjectUrl = PROJECT_URL,
		IconUrl = ICON_URL,
		LicenseUrl = LICENSE_URL,
		RequireLicenseAcceptance = false,
		Copyright = COPYRIGHT,
		ReleaseNotes = RELEASE_NOTES,
		Tags = TAGS,
		//Language = "en-US",
		OutputDirectory = parameters.PackageDirectory,
		Repository = new NuGetRepository {
			Type = "git",
			Url = GITHUB_SITE
		},
		Files = new [] {
			new NuSpecContent { Source = parameters.ProjectDirectory + "LICENSE.txt" },
			new NuSpecContent { Source = parameters.ProjectDirectory + "CHANGES.txt" },
			new NuSpecContent { Source = parameters.OutputDirectory + "vs-project-loader.dll", Target = "tools" }
		}
	});
}

//////////////////////////////////////////////////////////////////////
// BUILD CHOCOLATEY PACKAGE
//////////////////////////////////////////////////////////////////////

public void BuildChocolateyPackage(BuildParameters parameters)
{
	ChocolateyPack(new ChocolateyPackSettings()
	{
		Id = CHOCO_ID,
		Version = parameters.PackageVersion,
		Title = TITLE,
		Authors = AUTHORS,
		Owners = OWNERS,
		Description = DESCRIPTION,
		Summary = SUMMARY,
		ProjectUrl = PROJECT_URL,
		IconUrl = ICON_URL,
		LicenseUrl = LICENSE_URL,
		RequireLicenseAcceptance = false,
		Copyright = COPYRIGHT,
		ProjectSourceUrl = PROJECT_SOURCE_URL,
		DocsUrl= DOCS_URL,
		BugTrackerUrl = BUG_TRACKER_URL,
		PackageSourceUrl = PACKAGE_SOURCE_URL,
		MailingListUrl = MAILING_LIST_URL,
		ReleaseNotes = RELEASE_NOTES,
		Tags = TAGS,
		//Language = "en-US",
		OutputDirectory = parameters.PackageDirectory,
		Files = new [] {
			new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "LICENSE.txt", Target = "tools" },
			new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "CHANGES.txt", Target = "tools" },
			new ChocolateyNuSpecContent { Source = parameters.ProjectDirectory + "VERIFICATION.txt", Target = "tools" },
			new ChocolateyNuSpecContent { Source = parameters.OutputDirectory + "vs-project-loader.dll", Target = "tools" }
		}
	});
}

private void PushNuGetPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	NuGetPush(package, new NuGetPushSettings() { ApiKey = apiKey, Source = url });
}

private void PushChocolateyPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	ChocolateyPush(package, new ChocolateyPushSettings() { ApiKey = apiKey, Source = url });
}

private void CheckPackageExists(FilePath package)
{
	if (!FileExists(package))
		throw new InvalidOperationException(
			$"Package not found: {package.GetFilename()}.\nCode may have changed since package was last built.");
}
