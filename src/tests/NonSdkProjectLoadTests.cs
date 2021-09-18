// ***********************************************************************
// Copyright (c) 2008-2014 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;
using System.Linq;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    [TestFixture]
    public class NonSdkProjectLoadTests : ProjectLoaderTests
    {
        [TestCase("nonsdk-csharp-sample.csproj", new string[] { "Debug", "Release" }, "csharp-sample")]
        [TestCase("nonsdk-csharp-sample.csproj", new string[] { "Debug", "Release" }, "csharp-sample")]
        [TestCase("nonsdk-csharp-missing-output-path.csproj", new string[] { "Debug", "Release" }, "MissingOutputPath")]
        [TestCase("nonsdk-csharp-xna-project.csproj", new string[] { "Debug|x86", "Release|x86" }, "XNAWindowsProject")]
        [TestCase("nonsdk-vb-sample.vbproj", new string[] { "Debug", "Release" }, "vb-sample")]
        [TestCase("nonsdk-jsharp-sample.vjsproj", new string[] { "Debug", "Release" }, "jsharp-sample")]
        [TestCase("nonsdk-fsharp-sample.fsproj", new string[] { "Debug", "Release" }, "fsharp-sample")]
        public void CanLoadVsProject(string resourceName, string[] configs, string assemblyName)
        {
            Assert.That(_loader.CanLoadFrom(resourceName));

            using (TestResource file = new TestResource(resourceName))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.That(project.ConfigNames, Is.EqualTo(configs));

                foreach (var config in configs)
                {
                    TestPackage package = project.GetTestPackage(config);

                    Assert.AreEqual(resourceName, package.Name);
                    Assert.AreEqual(1, package.SubPackages.Count);
                    Assert.AreEqual(assemblyName, Path.GetFileNameWithoutExtension(package.SubPackages[0].FullName));
                    Assert.That(package.Settings.ContainsKey("BasePath"));
                    Assert.That(Path.GetDirectoryName(package.SubPackages[0].FullName), Is.SamePath((string)package.Settings["BasePath"]));
                }
            }
        }

        [TestCase("nonsdk-csharp-missing-assembly-name.csproj", "nonsdk-csharp-missing-assembly-name.exe")]
        [TestCase("nonsdk-csharp-missing-output-type.csproj", "MissingOutputType.dll")]
        public void PicksUpCorrectMsBuildProperty(string resourceName, string expectedOutputFilename)
        {
            using (TestResource file = new TestResource(resourceName))
            {
                IProject project = _loader.LoadFrom(file.Path);

                foreach (var config in project.ConfigNames)
                {
                    TestPackage package = project.GetTestPackage(config);

                    Assert.AreEqual(Path.GetFileName(package.SubPackages[0].FullName), expectedOutputFilename);
                }
            }
        }

        [Test]
        public void FromProjectWithTemplatedPaths()
        {
            using (var file = new TestResource("TestTemplatedPaths.csproj", NormalizePath(@"csharp-sample\csharp-sample.csproj")))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(3, project.ConfigNames.Count);

                var debugPackage = project.GetTestPackage("Debug");
                Assert.AreEqual(1, debugPackage.SubPackages.Count, "Debug should have 1 assembly");

                Assert.That(debugPackage.SubPackages[0].FullName, Does.Not.Contain(@"$(Configuration)"),
                    "Assembly path contains '$(Configuration)' which should be replaced with config name.");

                Assert.That(debugPackage.SubPackages[0].FullName, Does.EndWith(NormalizePath(@"\csharp-sample\.bin\Debug\TestTemplatedPathsAssembly\TestTemplatedPathsAssembly.dll")),
                    "Invalid Debug assembly path");

                var releasePackage = project.GetTestPackage("Release");
                Assert.AreEqual(1, releasePackage.SubPackages.Count, "Release should have 1 assemblies");
                Assert.That(releasePackage.SubPackages[0].FullName, Does.EndWith(NormalizePath(@"\csharp-sample\.bin\Release\TestTemplatedPathsAssembly\TestTemplatedPathsAssembly.dll")),
                    "Invalid Release assembly path");

                var fixedPathPackage = project.GetTestPackage("FixedPath");
                Assert.AreEqual(1, fixedPathPackage.SubPackages.Count, "FixedPath should have 1 assemblies");
                Assert.That(fixedPathPackage.SubPackages[0].FullName, Does.EndWith(NormalizePath(@"\csharp-sample\FixedPath\TestTemplatedPathsAssembly.dll")),
                    "Invalid FixedPath assembly path");
            }
        }

        [Test]
        public void ProjectWithDuplicatedSections()
        {
            using (var file = new TestResource("test-duplicated-key-project.csproj", NormalizePath(@"csharp-sample\csharp-sample.csproj")))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(3, project.ConfigNames.Count);

                var debugPackage = project.GetTestPackage("Debug");
                Assert.AreEqual(1, debugPackage.SubPackages.Count, "Debug should have 1 assembly");

                var releasePackage = project.GetTestPackage("Release");
                Assert.AreEqual(1, releasePackage.SubPackages.Count, "Release should have 1 assembly");

                var secondDebugPackage = project.GetTestPackage("Debug2ndTest");
                Assert.AreEqual(1, secondDebugPackage.SubPackages.Count, "Debug2ndTest should have 1 assembly");
                Assert.AreEqual(1, secondDebugPackage.SubPackages.Count, "Debug2ndTest should have 1 assemblies");
                Assert.That(secondDebugPackage.SubPackages[0].FullName, Does.EndWith(NormalizePath(@"\csharp-sample\Debug2ndTest\SecondTest\Test.exe")),
                    "Invalid Debug2ndTest assembly path");
            }
        }
    }
}
