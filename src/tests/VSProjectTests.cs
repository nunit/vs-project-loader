// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
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
