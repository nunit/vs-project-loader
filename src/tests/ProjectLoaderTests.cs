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
using NUnit.Engine.Extensibility;
using NUnit.Framework;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    [TestFixture]
    public static class ProjectLoaderTests
    {
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

    }
}
