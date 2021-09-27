// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System.IO;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Framework;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    [TestFixture]
    public class LegacyProjectLoadTests : ProjectLoadTests
    {
        static ProjectData[] LegacyProjects = new[]
        {
            new ProjectData("legacy-hebrew-file-problem.csproj")
                .Named("HebrewFileProblem"),
            new ProjectData("legacy-library-with-macros.vcproj")
                .WithConfig("Debug|Win32", "Debug/legacy-library-with-macros.dll")
                .WithConfig("Release|Win32", "Release/legacy-library-with-macros.dll"),
            new ProjectData("legacy-makefile-project.vcproj")
                .Named("MakeFileProject")
                .WithConfig("Debug|Win32", "Debug/MakeFileProject.exe")
                .WithConfig("Release|Win32", "Release/MakeFileProject.exe"),
            new ProjectData("legacy-sample.csproj")
                .Named("csharp-sample"),
            new ProjectData("legacy-sample.vbproj")
                .Named("vb-sample")
                .WithConfig("Debug", "bin/vb-sample.dll")
                .WithConfig("Release", "bin/vb-sample.dll"),
            new ProjectData("legacy-sample.vcproj")
                .Named("cpp-sample")
                .WithConfig("Debug|Win32", "Debug/cpp-sample.dll")
                .WithConfig("Release|Win32", "Release/cpp-sample.dll"),
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
