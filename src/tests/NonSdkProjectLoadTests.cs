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
    public class NonSdkProjectLoadTests : ProjectLoadTests
    {
        static ProjectData[] NonSdkProjects = new[]
        {
            new ProjectData("nonsdk-debug-only.csproj")
                .Named("DebugOnly"),
            new ProjectData("nonsdk-debug-only-no-nunit.csproj")
                .Named("DebugOnly"),
            new ProjectData("nonsdk-duplicated-key.csproj")
                .Named("Test")
                .WithConfig("Debug", ".bin/Debug/Test/Test.exe")
                .WithConfig("Release", ".bin/Release/Test//Test.exe")
                .WithConfig("Debug2ndTest", "Debug2ndTest/SecondTest//Test.exe"),
            new ProjectData("nonsdk-missing-assembly-name.csproj")
                .WithConfig("Debug", "bin/Common/nonsdk-missing-assembly-name.exe")
                .WithConfig("Release", "bin/Common/nonsdk-missing-assembly-name.exe"),
            new ProjectData("nonsdk-missing-output-path.csproj")
                .Named("MissingOutputPath")
                .WithConfig("Debug", "bin/Common/MissingOutputPath.dll")
                .WithConfig("Release", "bin/Common/MissingOutputPath.dll"),
            new ProjectData("nonsdk-missing-output-type.csproj")
                .Named("MissingOutputType")
                .WithConfig("Debug", "bin/Common/MissingOutputType.dll")
                .WithConfig("Release", "bin/Common/MissingOutputType.dll"),
            new ProjectData("nonsdk-multiple-platforms.csproj")
                .Named("MultiplePlatformProject")
                .WithConfig("Debug", "bin/Debug/MultiplePlatformProject.dll", "bin/x64/Debug/MultiplePlatformProject.dll", "bin/x86/Debug//MultiplePlatformProject.dll")
                .WithConfig("Release", "bin/Release/MultiplePlatformProject.dll", "bin/x64/Release/MultiplePlatformProject.dll", "bin/x86/Release/MultiplePlatformProject.dll"),
            new ProjectData("nonsdk-package-reference.csproj")
                .Named("project-with-package-reference"),
            new ProjectData("nonsdk-sample.csproj")
                .Named("csharp-sample"),
            new ProjectData("nonsdk-sample.fsproj")
                .Named("fsharp-sample"),
            new ProjectData("nonsdk-sample.vbproj")
                .Named("vb-sample")
                .WithConfig("Debug", "bin/vb-sample.dll")
                .WithConfig("Release", "bin/vb-sample.dll"),
            new ProjectData("nonsdk-sample.vjsproj")
                .Named("jsharp-sample"),
            new ProjectData("nonsdk-sample-noplatform.csproj")
                .Named("csharp-sample_VS2005_noplatform"),
            new ProjectData("nonsdk-templated-paths.csproj")
                .Named("TestTemplatedPathsAssembly")
                .WithConfig("Debug", ".bin/Debug/TestTemplatedPathsAssembly/TestTemplatedPathsAssembly.dll")
                .WithConfig("Release", ".bin/Release/TestTemplatedPathsAssembly/TestTemplatedPathsAssembly.dll")
                .WithConfig("FixedPath", "FixedPath//TestTemplatedPathsAssembly.dll"),
            new ProjectData("nonsdk-x86-only.csproj")
                .WithConfig("Debug", "bin/x86/Debug/DD.Net_46.Tests.dll")
                .WithConfig("Release", "bin/x86/Release/DD.Net_46.Tests.dll"),
            new ProjectData("nonsdk-xna-project.csproj")
                .Named("XNAWindowsProject")
                .WithConfig("Debug", "bin/x86/Debug/XNAWindowsProject.exe")
                .WithConfig("Release", "bin/x86/Release/XNAWindowsProject.exe")
        };

        [TestCaseSource(nameof(NonSdkProjects))]
        public void CanLoadNonSdkProject(ProjectData projectData)
        {
            CanLoadProject(projectData);
        }
    }
}
