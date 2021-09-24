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
using NUnit.Framework;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    [TestFixture]
    public class VSProjectTests
    {
        private static readonly string INVALID_FILE = Path.Combine(Path.GetTempPath(), "invalid.csproj");

        private void WriteInvalidFile( string text )
        {
            StreamWriter writer = new StreamWriter( INVALID_FILE );
            writer.WriteLine( text );
            writer.Close();
        }

        [TearDown]
        public void EraseInvalidFile()
        {
            if ( File.Exists( INVALID_FILE ) )
                File.Delete( INVALID_FILE );
        }

        [Test]
        public void FileNotFoundError()
        {
            Assert.Throws<FileNotFoundException>(() => new VSProject(@"/junk.csproj"));
        }

        [Test]
        public void InvalidXmlFormat()
        {
            WriteInvalidFile("<VisualStudioProject><junk></VisualStudioProject>");
            Assert.Throws<ArgumentException>(() => new VSProject(Path.Combine(Path.GetTempPath(), "invalid.csproj")));
        }

        [Test]
        public void EmptyProject()
        {
            WriteInvalidFile("<VisualStudioProject><junk></junk></VisualStudioProject>");
            VSProject project = new VSProject(Path.Combine(Path.GetTempPath(), "invalid.csproj"));
            Assert.AreEqual(0, project.ConfigNames.Count);
        }

        [Test]
        public void NoConfigurations()
        {
            WriteInvalidFile("<VisualStudioProject><CSharp><Build><Settings AssemblyName=\"invalid\" OutputType=\"Library\"></Settings></Build></CSharp></VisualStudioProject>");
            VSProject project = new VSProject(Path.Combine(Path.GetTempPath(), "invalid.csproj"));
            Assert.AreEqual(0, project.ConfigNames.Count);
        }
    }
}
