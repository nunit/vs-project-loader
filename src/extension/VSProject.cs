// ***********************************************************************
// Copyright (c) 2002-2014 Charlie Poole
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

using System.Collections.Generic;
using System.IO;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.ProjectLoaders
{
    /// <summary>
    /// This class allows loading information about
    /// configurations and assemblies in a Visual
    /// Studio project file and inspecting them.
    /// Only the most common project types are
    /// supported and an exception is thrown if
    /// an attempt is made to load an invalid
    /// file or one of an unknown type.
    /// </summary>
    public class VSProject : IProject
    {
        #region Static and Instance Variables

        /// <summary>
        /// The list of all our configs
        /// </summary>
        private IDictionary<string, ProjectConfig> ProjectConfigs = new Dictionary<string, ProjectConfig>();

        #endregion

        #region Constructor

        public VSProject(string projectPath)
        {
            ProjectPath = Path.GetFullPath(projectPath);
        }

        #endregion

        #region IProject Members

        /// <summary>
        /// The path to the project
        /// </summary>
        public string ProjectPath { get; private set; }

        /// <summary>
        /// Gets the active configuration, as defined
        /// by the particular project. For a VS
        /// project, we use the first config found.
        /// </summary>
        public string ActiveConfigName
        {
            get
            {
                var names = ConfigNames;
                return names.Count > 0 ? names[0] : null;
            }
        }

        public IList<string> ConfigNames
        {
            get
            {
                var names = new List<string>();
                foreach (var name in ProjectConfigs.Keys)
                    names.Add(name);
                return names;
            }
        }

        public TestPackage GetTestPackage()
        {
            return GetTestPackage(null);
        }

        public TestPackage GetTestPackage(string configName)
        {
            TestPackage package = new TestPackage(ProjectPath);

            foreach (var name in ProjectConfigs.Keys)
            {
                if (configName == name)
                {
                    var config = ProjectConfigs[configName];
                    foreach(string assemblyPath in config.AssemblyPaths)
                        package.AddSubPackage(new TestPackage(assemblyPath));
                    break;
                }
            }

            return package;
        }

        #endregion

        #region Other Public Properties and Methods

        /// <summary>
        /// The name of the project.
        /// </summary>
        public string Name => Path.GetFileNameWithoutExtension(ProjectPath);

        public string ProjectDir => Path.GetDirectoryName(ProjectPath);

        public bool HasConfig(string configName) => ProjectConfigs.ContainsKey(configName);

        public void AddConfig(string configName, string assemblyPath)
        {
            assemblyPath = Path.Combine(ProjectDir, assemblyPath);

            if (!HasConfig(configName))
                ProjectConfigs.Add(configName, new ProjectConfig(configName, assemblyPath));
            else
            {
                var config = ProjectConfigs[configName];
                if (!config.AssemblyPaths.Contains(assemblyPath))
                    config.AssemblyPaths.Add(assemblyPath);
            }
        }

        #endregion

        #region Nested ProjectConfig Class

        private class ProjectConfig
        {
            public ProjectConfig(string name, params string[] assemblyPaths)
            {
                Name = name;
                foreach (string path in assemblyPaths)
                    AssemblyPaths.Add(Normalize(path));
            }

            public string Name { get; }

            public string OutputDirectory { get; }

            public List<string> AssemblyPaths { get; } = new List<string>();

            private static string Normalize(string path)
            {
                char sep = Path.DirectorySeparatorChar;

                if (sep != '\\')
                    path = path.Replace('\\', sep);

                return path;
            }
        }

        #endregion
    }
}
