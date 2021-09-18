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
    public class LegacyProjectLoadTests
    {
        private readonly Regex PathSeparatorLookup = new Regex(@"[/\\]");
        private VisualStudioProjectLoader _loader;

        [SetUp]
        public void CreateLoader()
        {
            _loader = new VisualStudioProjectLoader();
        }

        [TestCase("legacy-csharp-sample.csproj", new string[] { "Debug", "Release" }, "csharp-sample")]
        [TestCase("legacy-csharp-hebrew-file-problem.csproj", new string[] { "Debug", "Release" }, "HebrewFileProblem")]
        [TestCase("legacy-vb-sample.vbproj", new string[] { "Debug", "Release" }, "vb-sample")]
        [TestCase("legacy-jsharp-sample.vjsproj", new string[] { "Debug", "Release" }, "jsharp-sample")]
        [TestCase("legacy-cpp-sample.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "cpp-sample")]
        [TestCase("legacy-cpp-library-with-macros.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "legacy-cpp-library-with-macros")]
        [TestCase("legacy-cpp-makefile-project.vcproj", new string[] { "Debug|Win32", "Release|Win32" }, "MakeFileProject")]
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

        private string NormalizePath(string path)
        {
            return this.PathSeparatorLookup.Replace(path, Path.DirectorySeparatorChar.ToString());
        }
    }
}
