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
    public class VisualStudioProjectLoaderTests
    {
        private readonly Regex PathSeparatorLookup = new Regex(@"[/\\]");
        private VisualStudioProjectLoader _loader;

        [SetUp]
        public void CreateLoader()
        {
            _loader = new VisualStudioProjectLoader();
        }

        [TestCase("project.csproj", true)]
        [TestCase("project.vbproj", true)]
        [TestCase("project.vjsproj", true)]
        [TestCase("project.fsproj", true)]
        [TestCase("project.vcproj", true)]
        [TestCase("project.sln", true)]
        [TestCase("project.xyproj", false)]
        public void ValidExtensions(string project, bool isGood)
        {
            if (isGood)
                Assert.That(_loader.CanLoadFrom(project), "Should be loadable: {0}", project);
            else
                Assert.False(_loader.CanLoadFrom(project), "Should not be loadable: {0}", project);
        }

        [Test]
        public void CheckExtensionAttribute()
        {
            Assert.That(typeof(VisualStudioProjectLoader),
                Has.Attribute<ExtensionAttribute>());
        }

        // Note for review:
        // The following test doesn't pass because AttributeConstraint always uses the
        // first attribute it finds. Should we fix the syntax or just document it?
        //[TestCase(".sln")]
        //[TestCase(".csproj")]
        //[TestCase(".vbproj")]
        //[TestCase(".vjsproj")]
        //[TestCase(".vcproj")]
        //[TestCase(".fsproj")]
        //public void CheckExtensionPropertyAttributes(string ext)
        //{
        //    Assert.That(typeof(VisualStudioProjectLoader),
        //        Has.Attribute<ExtensionPropertyAttribute>()
        //            .With.Property("Name").EqualTo("FileExtension").And.Property("Value").EqualTo(ext));
        //}

        [TestCase(".sln")]
        [TestCase(".csproj")]
        [TestCase(".vbproj")]
        [TestCase(".vjsproj")]
        [TestCase(".vcproj")]
        [TestCase(".fsproj")]
        public void CheckExtensionPropertyAttributes(string ext)
        {
            var attrs = typeof(VisualStudioProjectLoader).GetCustomAttributes(typeof(ExtensionPropertyAttribute), false);

            Assert.That(attrs,
                Has.Exactly(1)
                    .With.Property("Name").EqualTo("FileExtension")
                    .And.Property("Value").EqualTo(ext));
        }

        [Test]
        public void CannotLoadWebProject()
        {
            Assert.IsFalse(_loader.CanLoadFrom(@"http://localhost/web.csproj"));
            Assert.IsFalse(_loader.CanLoadFrom(@"\MyProject\http://localhost/web.csproj"));
        }

        [TestCase("csharp-sample.csproj", new string[] { "Debug", "Release" }, "csharp-sample")]
        [TestCase("csharp-sample.csproj", new string[] { "Debug", "Release" }, "csharp-sample")]
        [TestCase("csharp-missing-output-path.csproj", new string[] { "Debug", "Release" }, "MissingOutputPath")]
        [TestCase("csharp-xna-project.csproj", new string[] { "Debug|x86", "Release|x86" }, "XNAWindowsProject")]
        [TestCase("vb-sample.vbproj", new string[] { "Debug", "Release" }, "vb-sample")]
        [TestCase("jsharp-sample.vjsproj", new string[] { "Debug", "Release" }, "jsharp-sample")]
        [TestCase("fsharp-sample.fsproj", new string[] { "Debug", "Release" }, "fsharp-sample")]
        [TestCase("cpp-sample.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "cpp-sample")]
        [TestCase("cpp-default-library.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "cpp-default-library")]
        [TestCase("legacy-csharp-sample.csproj", new string[] { "Debug", "Release" }, "csharp-sample")]
        [TestCase("legacy-csharp-hebrew-file-problem.csproj", new string[] { "Debug", "Release" }, "HebrewFileProblem")]
        [TestCase("legacy-vb-sample.vbproj", new string[] { "Debug", "Release" }, "vb-sample")]
        [TestCase("legacy-jsharp-sample.vjsproj", new string[] { "Debug", "Release" }, "jsharp-sample")]
        [TestCase("legacy-cpp-sample.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "cpp-sample")]
        [TestCase("legacy-cpp-library-with-macros.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "legacy-cpp-library-with-macros")]
        [TestCase("legacy-cpp-makefile-project.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "MakeFileProject")]
        [TestCase("netcoreapp1.1-minimal.csproj", new string[] { "Debug", "Release" }, "netcoreapp1.1-minimal")]
        [TestCase("net20-minimal.csproj", new string[] { "Debug", "Release" }, "net20-minimal")]
        [TestCase("net20-with-assembly-name.csproj", new string[] { "Debug", "Release" }, "the-assembly-name")]
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

        [TestCase("netcoreapp1.1-minimal.csproj", "Debug", @"bin/Debug/netcoreapp1.1")]
        [TestCase("netcoreapp1.1-minimal.csproj", "Release", @"bin/Release/netcoreapp1.1")]
        [TestCase("netcoreapp1.1-with-output-path.csproj", "Debug", @"bin/Debug/netcoreapp1.1")]
        [TestCase("netcoreapp1.1-with-output-path.csproj", "Release", @"bin/Release/special")]
        [TestCase("net20-with-output-path.csproj", "Release", @"bin/Release/net20")]
        [TestCase("net20-with-output-path-no-target-framework.csproj", "Release", @"bin/Release")]
        public void PicksUpCorrectOutputPath(string resourceName, string configuration, string expectedOutputPath)
        {
            using (TestResource file = new TestResource(resourceName))
            {
                IProject project = _loader.LoadFrom(file.Path);

                var package = project.GetTestPackage(configuration);
                // adjust for difference between Linux/Win:
                var basePath = package.Settings["BasePath"].ToString().Replace('\\', '/');
                Console.WriteLine("BasePath: " + basePath);
                Assert.That(basePath.EndsWith(expectedOutputPath));
            }
        }

        [TestCase("netcoreapp1.1-minimal.csproj", "netcoreapp1.1-minimal.dll")]
        [TestCase("netcoreapp1.1-with-assembly-name.csproj", "the-assembly-name.dll")]
        [TestCase("netcoreapp2.0-minimal-dll.csproj", "netcoreapp2.0-minimal-dll.dll")]
        [TestCase("netcoreapp2.0-minimal-exe.csproj", "netcoreapp2.0-minimal-exe.dll")]
        [TestCase("netcoreapp2.0-minimal-web.csproj", "netcoreapp2.0-minimal-web.dll")]
        [TestCase("netcoreapp2.0-with-assembly-name-exe.csproj", "the-assembly-name.dll")]
        [TestCase("netcorefmwk-minimal-exe.csproj", "netcorefmwk-minimal-exe.exe")]
        [TestCase("netcorefmwk-minimal-web.csproj", "netcorefmwk-minimal-web.exe")]
        [TestCase("netcorefmwk-with-assembly-name-dll.csproj", "the-assembly-name.dll")]
        [TestCase("netcorefmwk-with-assembly-name-exe.csproj", "the-assembly-name.exe")]
        [TestCase("netstd2.0-minimal-dll.csproj", "netstd2.0-minimal-dll.dll")]
        [TestCase("netstd2.0-minimal-exe.csproj", "netstd2.0-minimal-exe.dll")]
        public void PicksUpCorrectAssemblyName(string resourceName, string expectedAssemblyName)
        {
            using (TestResource file = new TestResource(resourceName))
            {
                IProject project = _loader.LoadFrom(file.Path);

                foreach(var config in project.ConfigNames)
                {
                    TestPackage package = project.GetTestPackage(config);

                    Assert.That(Path.GetFileName(package.SubPackages[0].FullName) == expectedAssemblyName);
                }
            }
        }

        [TestCase("csharp-missing-assembly-name.csproj", "csharp-missing-assembly-name.exe")]
        [TestCase("csharp-missing-output-type.csproj", "MissingOutputType.dll")]
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
        public void FromVSSolution2003()
        {
            using (new TestResource("legacy-csharp-sample.csproj", NormalizePath(@"csharp\legacy-csharp-sample.csproj")))
            using (new TestResource("legacy-jsharp-sample.vjsproj", NormalizePath(@"jsharp\legacy-jsharp-sample.vjsproj")))
            using (new TestResource("legacy-vb-sample.vbproj", NormalizePath(@"vb\legacy-vb-sample.vbproj")))
            using (new TestResource("legacy-cpp-sample.vcproj", NormalizePath(@"cpp-sample\legacy-cpp-sample.vcproj")))
            using (TestResource file = new TestResource("legacy-samples.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(4, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(4, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void FromVSSolution2005()
        {
            using (new TestResource("csharp-sample.csproj", NormalizePath(@"csharp\csharp-sample.csproj")))
            using (new TestResource("jsharp-sample.vjsproj", NormalizePath(@"jsharp\jsharp-sample.vjsproj")))
            using (new TestResource("vb-sample.vbproj", NormalizePath(@"vb\vb-sample.vbproj")))
            using (new TestResource("cpp-sample.vcproj", NormalizePath(@"cpp-sample\cpp-sample.vcproj")))
            using (TestResource file = new TestResource("samples.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(4, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(4, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void FromWebApplication()
        {
            using (new TestResource("legacy-csharp-sample.csproj", NormalizePath(@"legacy-csharp-sample\legacy-csharp-sample.csproj")))
            using (TestResource file = new TestResource("solution-with-web-application.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(1, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(1, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void WithUnmanagedCpp()
        {
            using (new TestResource("legacy-csharp-sample.csproj", NormalizePath(@"legacy-csharp-sample\legacy-csharp-sample.csproj")))
            using (new TestResource("legacy-cpp-unmanaged.vcproj", NormalizePath(@"legacy-cpp-unmanaged\legacy-cpp-unmanaged.vcproj")))
            using (TestResource file = new TestResource("solution-with-unmanaged-cpp.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(2, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(2, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void FromSolutionWithDisabledProject()
        {
            using (new TestResource("csharp-sample.csproj", NormalizePath(@"csharp-sample\csharp-sample.csproj")))
            using (new TestResource("csharp-debug-only.csproj", NormalizePath(@"csharp-debug-only\csharp-debug-only.csproj")))
            using (TestResource file = new TestResource("solution-with-disabled-project.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(2, project.GetTestPackage("Release").SubPackages.Count, "Release should have 2 assemblies");
                Assert.AreEqual(1, project.GetTestPackage("Debug").SubPackages.Count, "Debug should have 1 assembly");
            }
        }

        [Test]
        public void FromSolutionWithNonNunitTestProject()
        {
            using (new TestResource("csharp-sample.csproj", NormalizePath(@"csharp-sample\csharp-sample.csproj")))
            using (new TestResource("csharp-debug-only-no-nunit.csproj", NormalizePath(@"csharp-debug-only-no-nunit\csharp-debug-only-no-nunit.csproj")))
            using (TestResource file = new TestResource("solution-with-non-nunit-project.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(2, project.GetTestPackage("Release").SubPackages.Count, "Release should have 2 assemblies");
                Assert.AreEqual(2, project.GetTestPackage("Debug").SubPackages.Count, "Debug should have 2 assemblies");
            }
        }

        [Test]
        public void FromSolutinoWithProjectUsingPackageReference()
        {
            using (new TestResource("project-with-package-reference.csproj", NormalizePath(@"project-with-package-reference\project-with-package-reference.csproj")))
            using(TestResource file = new TestResource("solution-with-package-reference.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(1, project.GetTestPackage("Release").SubPackages.Count, "Release should have 1 assemblies");
                Assert.AreEqual(1, project.GetTestPackage("Debug").SubPackages.Count, "Debug should have 1 assembly");
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

        private string NormalizePath(string path)
        {
            return this.PathSeparatorLookup.Replace(path, Path.DirectorySeparatorChar.ToString());
        }
    }
}
