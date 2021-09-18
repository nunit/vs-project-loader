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
    public class SolutionLoadTests : ProjectLoaderTests
    {
        [Test]
        public void VS2003Solution()
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
        public void VS2005Solution()
        {
            using (new TestResource("nonsdk-csharp-sample.csproj", NormalizePath(@"csharp\nonsdk-csharp-sample.csproj")))
            using (new TestResource("nonsdk-jsharp-sample.vjsproj", NormalizePath(@"jsharp\nonsdk-jsharp-sample.vjsproj")))
            using (new TestResource("nonsdk-vb-sample.vbproj", NormalizePath(@"vb\nonsdk-vb-sample.vbproj")))
            using (new TestResource("legacy-cpp-sample.vcproj", NormalizePath(@"cpp-sample\legacy-cpp-sample.vcproj")))
            using (TestResource file = new TestResource("solution-vs2005-samples.sln"))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.AreEqual(2, project.ConfigNames.Count);
                Assert.AreEqual(4, project.GetTestPackage("Debug").SubPackages.Count);
                Assert.AreEqual(4, project.GetTestPackage("Release").SubPackages.Count);
            }
        }

        [Test]
        public void SolutionWithWebApplication()
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
        public void SolutionWithUnmanagedCpp()
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
        public void SolutionWithDisabledProject()
        {
            using (new TestResource("nonsdk-csharp-sample.csproj", NormalizePath(@"csharp-sample\nonsdk-csharp-sample.csproj")))
            using (new TestResource("nonsdk-csharp-debug-only.csproj", NormalizePath(@"csharp-debug-only\nonsdk-csharp-debug-only.csproj")))
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
            using (new TestResource("nonsdk-csharp-sample.csproj", NormalizePath(@"csharp-sample\nonsdk-csharp-sample.csproj")))
            using (new TestResource("nonsdk-csharp-debug-only-no-nunit.csproj", NormalizePath(@"nonsdk-csharp-debug-only-no-nunit\csharp-debug-only-no-nunit.csproj")))
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
            using (new TestResource("project-with-package-reference.csproj", NormalizePath(@"project-with-package-reference\project-with-package-reference.csproj")))
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
