using System.Xml;

//////////////////////////////////////////////////////////////////////
// PACKAGE TEST
//////////////////////////////////////////////////////////////////////

public class PackageTest
{
	public string Description { get; set; }
	public string[] TestConsoleVersions { get; set; }
	public string Arguments { get; set; }
	public ExpectedResult ExpectedResult { get; set; }
}

//////////////////////////////////////////////////////////////////////
// PACKAGE TESTER
//////////////////////////////////////////////////////////////////////

const string DEFAULT_TEST_RESULT_FILE = "TestResult.xml";

public abstract class PackageTester
{
	protected BuildParameters _parameters;
	protected ICakeContext _context;

	public PackageTester(BuildParameters parameters)
    {
		_parameters = parameters;
		_context = parameters.Context;

        // TODO: Re-enable when issue #38 and general support of the 
        // new SDK format are complete.

        PackageTests.Add(new PackageTest()
        {
            Description = "Re-run unit tests using csproj file",
            Arguments = $"src/tests/vs-project-loader.tests.csproj --config={_parameters.Configuration}",
            TestConsoleVersions = new string[] { "3.7.0" },
            ExpectedResult = new ExpectedResult("Passed")
            {
                Total = 69,
                Passed = 69,
                Failed = 0,
                Warnings = 0,
                Inconclusive = 0,
                Skipped = 0,
                Assemblies = new[] { new ExpectedAssemblyResult("vs-project-loader.tests.dll", "net-2.0") }
            }
        });
    }

	protected abstract string PackageName { get; }
	protected abstract string PackageUnderTest { get; }
	public abstract string InstallDirectory { get; }

	public PackageCheck[] PackageChecks { get; set; }
	public List<PackageTest> PackageTests = new List<PackageTest>();

	public void RunPackageTests()
    {
		if (PackageTests.Count == 0)
		{
			Console.WriteLine("No package tests were found.");
			return;
		}

		var reporter = new ResultReporter(PackageName);

		foreach (var packageTest in PackageTests)
		{
			var resultFile = _parameters.ProjectDirectory + DEFAULT_TEST_RESULT_FILE;

			foreach (var consoleVersion in packageTest.TestConsoleVersions)
			{
				// Delete result file ahead of time so we don't mistakenly
				// read a left-over file from another test run. Leave the
				// file after the run in case we need it to debug a failure.
				if (_context.FileExists(resultFile))
					_context.DeleteFile(resultFile);

				DisplayBanner(packageTest.Description + " - Console Version " + consoleVersion);

				RunConsoleTest(consoleVersion, packageTest.Arguments);

				try
				{
					var result = new ActualResult(resultFile);
					var report = new PackageTestReport(packageTest, consoleVersion, result);
					reporter.AddReport(report);

					Console.WriteLine(report.Errors.Count == 0
						? "\nSUCCESS: Test Result matches expected result!"
						: "\nERROR: Test Result not as expected!");
				}
				catch (Exception ex)
				{
					reporter.AddReport(new PackageTestReport(packageTest, consoleVersion, ex));

					Console.WriteLine($"\nERROR: No result found.");
					Console.WriteLine(ex.ToString());
				}
			}
		}

		bool anyErrors = reporter.ReportResults();
		Console.WriteLine();

		// All package tests are run even if one of them fails. If there are
		// any errors,  we stop the run at this point.
		if (anyErrors)
			throw new Exception("One or more package tests had errors!");
	}

	private void RunConsoleTest(string consoleVersion, string arguments)
    {
		string runner = _parameters.GetPathToConsoleRunner(consoleVersion);

		if (InstallDirectory.EndsWith(CHOCO_ID + "/"))
		{
			// We are using nuget packages for the runner, so add an extra
			// addins file to allow detecting chocolatey packages
			string runnerDir = System.IO.Path.GetDirectoryName(runner);
			using (var writer = new StreamWriter(runnerDir + "/choco.engine.addins"))
				writer.WriteLine("../../nunit-extension-*/tools/");
		}

		_context.StartProcess(runner, arguments);
		// We don't check the error code because we know that
		// mock-assembly returns -4 due to a bad fixture.

		// Should have created the result file
		if (!_context.FileExists(DEFAULT_TEST_RESULT_FILE))
			throw new System.Exception("The result file was not created.");
	}

	private void DisplayBanner(string message)
	{
		Console.WriteLine();
		Console.WriteLine("=======================================================");
		Console.WriteLine(message);
		Console.WriteLine("=======================================================");
	}
}

public class NuGetPackageTester : PackageTester
{
    public NuGetPackageTester(BuildParameters parameters) : base(parameters) { }

	protected override string PackageName => _parameters.NuGetPackageName;
	protected override string PackageUnderTest => _parameters.NuGetPackage;
	public override string InstallDirectory => _parameters.NuGetInstallDirectory;
}

public class ChocolateyPackageTester : PackageTester
{
    public ChocolateyPackageTester(BuildParameters parameters) : base(parameters) { }

	protected override string PackageName => _parameters.ChocolateyPackageName;
	protected override string PackageUnderTest => _parameters.ChocolateyPackage;
	public override string InstallDirectory => _parameters.ChocolateyInstallDirectory;
}
