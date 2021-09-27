// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.IO;
using NUnit.Engine.Extensibility;
using NUnit.Framework;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    [TestFixture]
    public static class ProjectLoaderTests
    {
        private static readonly string INVALID_FILE = Path.Combine(Path.GetTempPath(), "invalid.csproj");

        private static void WriteInvalidFile(string text)
        {
            StreamWriter writer = new StreamWriter(INVALID_FILE);
            writer.WriteLine(text);
            writer.Close();
        }

        [TearDown]
        public static void EraseInvalidFile()
        {
            if (File.Exists(INVALID_FILE))
                File.Delete(INVALID_FILE);
        }

        [Test]
        public static void CheckExtensionAttribute()
        {
            Assert.That(typeof(VisualStudioProjectLoader),
                Has.Attribute<ExtensionAttribute>());
        }

        [TestCase(".sln")]
        [TestCase(".csproj")]
        [TestCase(".vbproj")]
        [TestCase(".vjsproj")]
        [TestCase(".vcproj")]
        [TestCase(".fsproj")]
        public static void CheckExtensionPropertyAttributes(string ext)
        {
            var attrs = typeof(VisualStudioProjectLoader).GetCustomAttributes(typeof(ExtensionPropertyAttribute), false);

            Assert.That(attrs,
                Has.Exactly(1)
                    .With.Property("Name").EqualTo("FileExtension")
                    .And.Property("Value").EqualTo(ext));
        }

        [TestCase("project.csproj", ExpectedResult = true)]
        [TestCase("project.vbproj", ExpectedResult = true)]
        [TestCase("project.vjsproj", ExpectedResult = true)]
        [TestCase("project.fsproj", ExpectedResult = true)]
        [TestCase("project.vcproj", ExpectedResult = true)]
        [TestCase("project.sln", ExpectedResult = true)]
        [TestCase("project.xyproj", ExpectedResult = false)]
        [TestCase("http://localhost/web.csproj", ExpectedResult = false)]
        [TestCase(@"\MyProject\http://localhost/web.csproj", ExpectedResult = false)]
        public static bool ValidExtensions(string project)
        {
            return new VisualStudioProjectLoader().CanLoadFrom(project);
        }

        [Test]
        public static void LoadInvalidFileType()
        {
            Assert.Throws<ArgumentException>(() => new VisualStudioProjectLoader().LoadFrom(@"/test.junk"));
        }

        [Test]
        public static void FileNotFoundError()
        {
            Assert.Throws<FileNotFoundException>(() => new VisualStudioProjectLoader().LoadFrom(@"/junk.csproj"));
        }

        [Test]
        public static void InvalidXmlFormat()
        {
            WriteInvalidFile("<VisualStudioProject><junk></VisualStudioProject>");
            Assert.Throws<ArgumentException>(() => new VisualStudioProjectLoader().LoadFrom(Path.Combine(Path.GetTempPath(), "invalid.csproj")));
        }

    }
}
