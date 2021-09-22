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

using System.IO;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Framework;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    [TestFixture]
    public class SolutionLoadTests : ProjectLoadTests
    {
        [Test]
        public void VS2003Solution()
        {
            using (new TestResource("legacy-sample.csproj", NormalizePath(@"csharp\legacy-sample.csproj")))
            using (new TestResource("legacy-sample.vjsproj", NormalizePath(@"jsharp\legacy-sample.vjsproj")))
            using (new TestResource("legacy-sample.vbproj", NormalizePath(@"vb\legacy-sample.vbproj")))
            using (new TestResource("legacy-sample.vcproj", NormalizePath(@"cpp-sample\legacy-sample.vcproj")))
            using (TestResource file = new TestResource("legacy-samples.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(4, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(4, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void VS2005Solution()
        {
            using (new TestResource("nonsdk-sample.csproj", NormalizePath(@"csharp\nonsdk-sample.csproj")))
            using (new TestResource("nonsdk-sample.vjsproj", NormalizePath(@"jsharp\nonsdk-sample.vjsproj")))
            using (new TestResource("nonsdk-sample.vbproj", NormalizePath(@"vb\nonsdk-sample.vbproj")))
            using (new TestResource("legacy-sample.vcproj", NormalizePath(@"cpp-sample\legacy-sample.vcproj")))
            using (TestResource file = new TestResource("solution-vs2005-samples.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(4, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(4, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        //[Test]
        public void SolutionWithMultiplePlatforms()
        {
            using (new TestResource("nonsdk-multiple-platforms.csproj"))
            using (TestResource file = new TestResource("solution-with-multiple-platforms.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.That(project.ConfigNames, Is.EqualTo(new[] { "Debug", "Release" }));

                string tempDir = Path.GetDirectoryName(file.Path);
                foreach (string config in project.ConfigNames)
                {
                    var subPackages = project.GetTestPackage(config).SubPackages;
                    Assert.That(subPackages.Count, Is.EqualTo(3));
                    Assert.That(subPackages[0].FullName, Is.EqualTo(
                        $"{tempDir}\\bin\\{config}\\MultiplePlatformProject.dll"));
                    Assert.That(subPackages[1].FullName, Is.EqualTo(
                        $"{tempDir}\\bin\\x64\\{config}\\MultiplePlatformProject.dll"));
                    Assert.That(subPackages[2].FullName, Is.EqualTo(
                        $"{tempDir}\\bin\\x86\\{config}\\MultiplePlatformProject.dll"));
                }
            }
        }

        [Test]
        public void SolutionWithWebApplication()
        {
            using (new TestResource("legacy-sample.csproj", NormalizePath(@"legacy-sample\legacy-sample.csproj")))
            using (TestResource file = new TestResource("solution-with-web-application.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(1, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(1, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void SolutionWithUnmanagedCpp()
        {
            using (new TestResource("legacy-sample.csproj", NormalizePath(@"legacy-sample\legacy-sample.csproj")))
            using (new TestResource("legacy-unmanaged.vcproj", NormalizePath(@"legacy-unmanaged\legacy-unmanaged.vcproj")))
            using (TestResource file = new TestResource("solution-with-unmanaged-cpp.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(2, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(2, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void SolutionWithDisabledProject()
        {
            using (new TestResource("nonsdk-sample.csproj", NormalizePath(@"csharp-sample\nonsdk-sample.csproj")))
            using (new TestResource("nonsdk-debug-only.csproj", NormalizePath(@"csharp-debug-only\nonsdk-debug-only.csproj")))
            using (TestResource file = new TestResource("solution-with-disabled-project.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(2, project.GetTestPackage("Release").SubPackages.Count, "Release should have 2 assemblies");
                Assert.AreEqual(1, project.GetTestPackage("Debug").SubPackages.Count, "Debug should have 1 assembly");
            }
        }

        [Test]
        public void SolutionWithNonNunitTestProject()
        {
            using (new TestResource("nonsdk-sample.csproj", NormalizePath(@"csharp-sample\nonsdk-sample.csproj")))
            using (new TestResource("nonsdk-debug-only-no-nunit.csproj", NormalizePath(@"debug-only-no-nunit\nonsdk-debug-only-no-nunit.csproj")))
            using (TestResource file = new TestResource("solution-with-non-nunit-project.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(2, project.GetTestPackage("Release").SubPackages.Count, "Release should have 2 assemblies");
                Assert.AreEqual(2, project.GetTestPackage("Debug").SubPackages.Count, "Debug should have 2 assemblies");
            }
        }

        [Test]
        public void SolutionWithProjectUsingPackageReference()
        {
            using (new TestResource("nonsdk-package-reference.csproj", NormalizePath(@"project-with-package-reference\nonsdk-package-reference.csproj")))
            using (TestResource file = new TestResource("solution-with-package-reference.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);
                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(1, project.GetTestPackage("Release").SubPackages.Count, "Release should have 1 assemblies");
                Assert.AreEqual(1, project.GetTestPackage("Debug").SubPackages.Count, "Debug should have 1 assembly");
            }
        }
    }
}
