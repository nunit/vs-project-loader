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
    public class LegacyProjectLoadTests : ProjectLoaderTests
    {
        static ProjectData[] LegacyProjects = new[]
        {
            new ProjectData("legacy-hebrew-file-problem.csproj")
                .Named("HebrewFileProblem"),
            new ProjectData("legacy-library-with-macros.vcproj")
                .WithConfig("Debug|Win32", "Debug/")
                .WithConfig("Release|Win32", "Release/"),
            new ProjectData("legacy-makefile-project.vcproj")
                .Named("MakeFileProject")
                .WithConfig("Debug|Win32", "Debug/")
                .WithConfig("Release|Win32", "Release/"),
            new ProjectData("legacy-sample.csproj")
                .Named("csharp-sample"),
            new ProjectData("legacy-sample.vbproj")
                .Named("vb-sample")
                .WithConfig("Debug", "bin/")
                .WithConfig("Release", "bin/"),
            new ProjectData("legacy-sample.vcproj")
                .Named("cpp-sample")
                .WithConfig("Debug|Win32", "Debug/")
                .WithConfig("Release|Win32", "Release/"),
            new ProjectData("legacy-sample.vjsproj")
                .Named("jsharp-sample")
        };

        [TestCaseSource(nameof(LegacyProjects))]
        public void CanLoadLegacyProject(ProjectData projectData)
        {
            CanLoadProject(projectData);
        }
    }
}
