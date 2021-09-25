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
    public class SdkProjectLoadTests : ProjectLoadTests
    {
        static ProjectData[] SdkProjects = new ProjectData[]
        {
            new ProjectData("sdk-net20-minimal.csproj")
                .RuntimeDirectory("net20"),
            new ProjectData("sdk-net20-minimal-with-target-frameworks.csproj")
                .RuntimeDirectory("net20"),
            new ProjectData("sdk-net20-with-assembly-name.csproj")
                .Named("the-assembly-name")
                .RuntimeDirectory("net20"),
            new ProjectData("sdk-net20-with-output-path.csproj")
                .RuntimeDirectory("net20"),
            new ProjectData("sdk-net20-with-output-path-no-target-framework.csproj")
                .WithConfig("Debug", "bin/output/sdk-net20-with-output-path-no-target-framework.dll")
                .WithConfig("Release", "bin/output/sdk-net20-with-output-path-no-target-framework.dll"),
            new ProjectData("sdk-net461-minimal-exe.csproj")
                .WithConfig("Debug", "bin/Debug/net461/sdk-net461-minimal-exe.exe")
                .WithConfig("Release", "bin/Release/net461/sdk-net461-minimal-exe.exe"),
            new ProjectData("sdk-net461-minimal-web.csproj")
                .WithConfig("Debug", "bin/Debug/net461/sdk-net461-minimal-web.exe")
                .WithConfig("Release", "bin/Release/net461/sdk-net461-minimal-web.exe"),
            new ProjectData("sdk-net461-with-assembly-name-dll.csproj")
                .Named("the-assembly-name")
                .RuntimeDirectory("net461"),
            new ProjectData("sdk-net461-with-assembly-name-exe.csproj")
                .WithConfig("Debug", "bin/Debug/net461/the-assembly-name.exe")
                .WithConfig("Release", "bin/Release/net461/the-assembly-name.exe"),
            new ProjectData("sdk-netcoreapp1.1-minimal.csproj")
                .RuntimeDirectory("netcoreapp1.1"),
            new ProjectData("sdk-netcoreapp1.1-with-assembly-name.csproj")
                .Named("the-assembly-name")
                .RuntimeDirectory("netcoreapp1.1"),
            new ProjectData("sdk-netcoreapp1.1-with-output-path.csproj")
                .WithConfig("Debug", "bin/Debug/netcoreapp1.1/sdk-netcoreapp1.1-with-output-path.dll")
                .WithConfig("Release", "bin/Release/special/netcoreapp1.1/sdk-netcoreapp1.1-with-output-path.dll"),
            new ProjectData("sdk-netcoreapp2.0-minimal-dll.csproj")
                .RuntimeDirectory("netcoreapp2.0"),
            new ProjectData("sdk-netcoreapp2.0-minimal-exe.csproj")
                .RuntimeDirectory("netcoreapp2.0"),
            new ProjectData("sdk-netcoreapp2.0-minimal-web.csproj")
                .RuntimeDirectory("netcoreapp2.0"),
            new ProjectData("sdk-netcoreapp2.1-with-assembly-name-exe.csproj")
                .Named("the-assembly-name")
                .RuntimeDirectory("netcoreapp2.1"),
            new ProjectData("sdk-netstandard2.0-minimal-dll.csproj")
                .RuntimeDirectory("netstandard2.0"),
            new ProjectData("sdk-netstandard2.0-minimal-exe.csproj")
                .RuntimeDirectory("netstandard2.0"),
            new ProjectData("sdk-net5.0-minimal-exe.fsproj")
                .WithConfig("Debug", "bin/Debug/net5.0/sdk-net5.0-minimal-exe.exe")
                .WithConfig("Release", "bin/Release/net5.0/sdk-net5.0-minimal-exe.exe"),
            new ProjectData("sdk-net5.0-minimal-exe.vbproj")
                .WithConfig("Debug", "bin/Debug/net5.0/sdk-net5.0-minimal-exe.exe")
                .WithConfig("Release", "bin/Release/net5.0/sdk-net5.0-minimal-exe.exe")
        };

        [TestCaseSource(nameof(SdkProjects))]
        public void CanLoadSdkProject(ProjectData projectData)
        {
            CanLoadProject(projectData);
        }
    }
}
