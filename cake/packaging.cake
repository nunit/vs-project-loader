//////////////////////////////////////////////////////////////////////
// PACKAGE METADATA
//////////////////////////////////////////////////////////////////////

const string TITLE = "NUnit 3 - Visual Studio Project Loader Extension";
static readonly string[] AUTHORS = new [] { "Charlie Poole" };
static readonly string[] OWNERS = new [] { "Charlie Poole" };
const string DESCRIPTION = "This extension allows NUnit to recognize and load solutions and projects in Visual Studio format. It supports files of type .sln, .csproj, .vbproj, .vjsproj, .vcproj and .fsproj.";
const string SUMMARY = "NUnit Engine extension for loading Visual Studio formatted projects.";
const string COPYRIGHT = "Copyright (c) 2018 Charlie Poole";
static readonly string[] RELEASE_NOTES = new [] { "See https://raw.githubusercontent.com/nunit/vs-project-loader/main/CHANGES.txt" };
static readonly string[] TAGS = new [] { "nunit", "test", "testing", "tdd", "runner" };
static readonly Uri PROJECT_URL = new Uri("http://nunit.org");
static readonly Uri ICON_URL = new Uri("https://cdn.rawgit.com/nunit/resources/master/images/icon/nunit_256.png");
static readonly Uri LICENSE_URL = new Uri("http://nunit.org/nuget/nunit3-license.txt");
const string GITHUB_SITE = "https://github.com/nunit/vs-project-loader";
const string WIKI_PAGE = "https://github.com/nunit/docs/wiki/Console-Command-Line";
static readonly Uri PROJECT_SOURCE_URL = new Uri( GITHUB_SITE );
static readonly Uri PACKAGE_SOURCE_URL = new Uri( GITHUB_SITE );
static readonly Uri BUG_TRACKER_URL = new Uri(GITHUB_SITE + "/issues");
static readonly Uri DOCS_URL = new Uri(WIKI_PAGE);
static readonly Uri MAILING_LIST_URL = new Uri("https://groups.google.com/forum/#!forum/nunit-discuss");

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
