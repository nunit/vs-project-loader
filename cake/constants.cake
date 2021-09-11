// This file contains both constants and static readonly values, which
// are used as constants. The latter must not depend in any way on the
// contents of other cake files, which are loaded after this one.

const string DEFAULT_CONFIGURATION = "Release";
const string DEFAULT_VERSION = "3.9.0";

// Files
const string SOLUTION_FILE = "vs-project-loader.sln";
const string OUTPUT_ASSEMBLY = "vs-project-loader.dll";
const string UNIT_TEST_ASSEMBLY = "vs-project-loader.tests.dll";
const string MOCK_ASSEMBLY = "mock-assembly.dll";

// Packaging
const string NUGET_ID = "NUnit.Extension.VSProjectLoader";
const string CHOCO_ID = "nunit-extension-vs-project-loader";
const string GITHUB_SITE = "https://github.com/nunit/vs-project-loader";
const string WIKI_PAGE = "https://github.com/nunit/docs/wiki/Console-Command-Line";

// URLs for uploading packages
const string MYGET_PUSH_URL = "https://www.myget.org/F/nunit/api/v2";
const string NUGET_PUSH_URL = "https://api.nuget.org/v3/index.json";
const string CHOCO_PUSH_URL = "https://push.chocolatey.org/";

// Environment Variable names holding API keys
const string MYGET_API_KEY = "MYGET_API_KEY";
const string NUGET_API_KEY = "NUGET_API_KEY";
const string CHOCO_API_KEY = "CHOCO_API_KEY";

// Environment Variable names holding GitHub identity of user
const string GITHUB_OWNER = "NUnit";
const string GITHUB_REPO = "nunit-project-loader";
// Access token is used by GitReleaseManager
const string GITHUB_ACCESS_TOKEN = "GITHUB_ACCESS_TOKEN";

// Pre-release labels that we publish
private static readonly string[] LABELS_WE_PUBLISH_ON_MYGET = { "dev", "pre" };
private static readonly string[] LABELS_WE_PUBLISH_ON_NUGET = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_PUBLISH_ON_CHOCOLATEY = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_RELEASE_ON_GITHUB = { "alpha", "beta", "rc" };

// Metadata used in the nuget and chocolatey packages
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

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var OUTPUT_DIR = PROJECT_DIR + "output/";

// Package sources for nuget restore
var PACKAGE_SOURCE = new string[]
	{
		"https://www.nuget.org/api/v2",
		"https://www.myget.org/F/nunit/api/v2"
	};
