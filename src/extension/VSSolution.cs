// ***********************************************************************
// Copyright (c) 2014 Charlie Poole
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
using System.Text.RegularExpressions;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.ProjectLoaders
{
    public class VSSolution : IProject
    {
        IDictionary<string, SolutionConfig> _configs = new Dictionary<string, SolutionConfig>();

        #region Constructor

        public VSSolution(string projectPath)
        {
            ProjectPath = Path.GetFullPath(projectPath);
        }

        #endregion

        #region IProject Members

        /// <summary>
        /// The path to the project
        /// </summary>
        public string ProjectPath { get; private set; }

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
                foreach (var name in _configs.Keys)
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
            var package = new TestPackage(ProjectPath);

            foreach (var name in _configs.Keys)
            {
                if (configName == null || configName == name)
                {
                    var config = _configs[name];
                    foreach (string assembly in config.Assemblies)
                    {
                        package.AddSubPackage(new TestPackage(assembly));
                    }
                    break;
                }
            }

            package.AddSetting("SkipNonTestAssemblies", true);

            return package;
        }

        #endregion

        #region Public Methods

        public void AddConfig(string name, string[] assemblies)
        {
            _configs.Add(name, new SolutionConfig(name, assemblies));
        }

        #endregion

        #region Nested SolutionConfig Class

        private class SolutionConfig
        {
            public SolutionConfig(string name, params string[] assemblies)
            {
                Name = name;
                Assemblies = new List<string>(assemblies);
            }

            public string Name { get; private set; }

            public IList<string> Assemblies { get; private set; }
        }

        #endregion
    }
}
