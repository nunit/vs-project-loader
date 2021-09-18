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
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Framework;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    [TestFixture]
    public class SdkProjectLoadTests : ProjectLoaderTests
    {
        [TestCase("sdk-netcoreapp1.1-minimal.csproj", new string[] { "Debug", "Release" }, "sdk-netcoreapp1.1-minimal")]
        [TestCase("sdk-net20-minimal.csproj", new string[] { "Debug", "Release" }, "sdk-net20-minimal")]
        [TestCase("sdk-net20-with-assembly-name.csproj", new string[] { "Debug", "Release" }, "the-assembly-name")]
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

        [TestCase("sdk-netcoreapp1.1-minimal.csproj", "Debug", @"bin/Debug/netcoreapp1.1")]
        [TestCase("sdk-netcoreapp1.1-minimal.csproj", "Release", @"bin/Release/netcoreapp1.1")]
        [TestCase("sdk-netcoreapp1.1-with-output-path.csproj", "Debug", @"bin/Debug/netcoreapp1.1")]
        [TestCase("sdk-netcoreapp1.1-with-output-path.csproj", "Release", @"bin/Release/special")]
        [TestCase("sdk-net20-with-output-path.csproj", "Release", @"bin/Release/net20")]
        [TestCase("sdk-net20-with-output-path-no-target-framework.csproj", "Release", @"bin/Release")]
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

        [TestCase("sdk-netcoreapp1.1-minimal.csproj", "sdk-netcoreapp1.1-minimal.dll")]
        [TestCase("sdk-netcoreapp1.1-with-assembly-name.csproj", "the-assembly-name.dll")]
        [TestCase("sdk-netcoreapp2.0-minimal-dll.csproj", "sdk-netcoreapp2.0-minimal-dll.dll")]
        [TestCase("sdk-netcoreapp2.0-minimal-exe.csproj", "sdk-netcoreapp2.0-minimal-exe.dll")]
        [TestCase("sdk-netcoreapp2.0-minimal-web.csproj", "sdk-netcoreapp2.0-minimal-web.dll")]
        [TestCase("sdk-netcoreapp2.0-with-assembly-name-exe.csproj", "the-assembly-name.dll")]
        [TestCase("sdk-net461-minimal-exe.csproj", "sdk-net461-minimal-exe.exe")]
        [TestCase("sdk-net461-minimal-web.csproj", "sdk-net461-minimal-web.exe")]
        [TestCase("sdk-net461-with-assembly-name-dll.csproj", "the-assembly-name.dll")]
        [TestCase("sdk-net461-with-assembly-name-exe.csproj", "the-assembly-name.exe")]
        [TestCase("sdk-netstandard2.0-minimal-dll.csproj", "sdk-netstandard2.0-minimal-dll.dll")]
        [TestCase("sdk-netstandard2.0-minimal-exe.csproj", "sdk-netstandard2.0-minimal-exe.dll")]
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
    }
}
