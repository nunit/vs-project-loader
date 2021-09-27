// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
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
