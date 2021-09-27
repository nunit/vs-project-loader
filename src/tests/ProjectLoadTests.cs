// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Engine.Extensibility;
using NUnit.Engine.Tests.resources;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System;

namespace NUnit.Engine.Services.ProjectLoaders.Tests
{
    public abstract class ProjectLoadTests
    {
        protected static readonly Regex PathSeparatorLookup = new Regex(@"[/\\]");
        protected VisualStudioProjectLoader _loader;

        [SetUp]
        public void CreateLoader()
        {
            _loader = new VisualStudioProjectLoader();
        }

        protected void CanLoadProject(ProjectData projectData)
        {
            Assert.That(_loader.CanLoadFrom(projectData.ProjectName), "Failed to load project");

            using (TestResource file = new TestResource(projectData.ProjectName))
            {
                IProject project = _loader.LoadFrom(file.Path);

                Assert.That(project.ConfigNames, Is.EquivalentTo(projectData.ConfigNames));

                foreach (var config in projectData.ConfigNames)
                {
                    TestPackage package = project.GetTestPackage(config);
                    Assert.That(package.Name, Is.EqualTo(projectData.ProjectName));

                    ConfigData configData = projectData.Configs[config];
                    string projectDir = Path.GetDirectoryName(file.Path);

                    int expectedCount = configData.AssemblyPaths.Length;
                    int actualCount = package.SubPackages.Count;

                    // Multiple Assert gives us more info on failures
                    Assert.Multiple(() =>
                    {
                        Assert.That(actualCount, Is.EqualTo(expectedCount), "Wrong number of subpackages");

                        for (int i = 0; i < expectedCount && i < actualCount; i++)
                        {
                            var subPackage = package.SubPackages[i];
                            string expectedPath = Path.Combine(projectDir, configData.AssemblyPaths[i]);
                            Assert.That(subPackage.FullName, Is.SamePath(expectedPath), $"Bad output path for {configData.Name} config");
                        }

                        if (TestContext.CurrentContext.Result.Outcome.Status == Framework.Interfaces.TestStatus.Failed)
                        {
                            Console.WriteLine("Expected SubPackages:");
                            for (int i = 0; i < expectedCount; i++)
                                Console.WriteLine("  " + configData.AssemblyPaths[i]);

                            Console.WriteLine("Actual SubPackages:");
                            for (int i = 0; i < actualCount; i++)
                                Console.WriteLine("  " + package.SubPackages[i].FullName);

                            Assert.Fail("See console output for more info");
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Nested ProjectData class
        /// </summary>
        public class ProjectData
        {
            private string[] DEFAULT_CONFIGS = new[] { "Debug", "Release" };
            private Dictionary<string, ConfigData> _configs;

            public ProjectData(string projectName, bool appendTargetRuntimes = false)
            {
                ProjectName = projectName;

                // Set default value. Set property to override.
                AssemblyName = Path.GetFileNameWithoutExtension(projectName);
            }

            public string ProjectName { get; }

            public IDictionary<string, ConfigData> Configs
            {
                get
                {
                    if (_configs == null)
                    {
                        // Set up default configs
                        _configs = new Dictionary<string, ConfigData>();

                        foreach (var config in DEFAULT_CONFIGS)
                        {
                            var outputPath = _runtimeDirectory != null
                                ? $"bin/{config}/{_runtimeDirectory}/"
                                : $"bin/{config}/";
                            _configs.Add(config, new ConfigData(config, $"{outputPath}/{AssemblyName}.dll"));
                        }
                    }

                    return _configs;
                }
            }

            public string[] ConfigNames => Configs.Keys.ToArray();
            public string AssemblyName { get; private set; }

            //////////////////////////////////////////////////////
            // Fluent property setters
            //////////////////////////////////////////////////////

            /// <summary>
            /// Specify a non-default assembly name
            /// </summary>
            /// <param name="name">The assemlby name</param>
            /// <returns>Self</returns>
            public ProjectData Named(string name)
            {
                AssemblyName = name;
                return this;
            }

            string _runtimeDirectory;
            public ProjectData RuntimeDirectory(string runtimeDirectory)
            {
                _runtimeDirectory = runtimeDirectory;
                return this;
            }

            /// <summary>
            /// Add a list of configs to the project data, using default output path.
            /// </summary>
            /// <param name="names">Config names</param>
            /// <returns>Self</returns>
            public ProjectData WithConfigs(params string[] names)
            {
                var result = this;

                foreach (string name in names)
                    result = result.WithConfig(name);

                return result; 
            }

            /// <summary>
            /// Add a config to the project data
            /// </summary>
            /// <param name="name">The name of the config</param>
            /// <param name="assemblyPaths">Optional path to the directory used for output.</param>
            /// <returns>Self</returns>
            public ProjectData WithConfig(string name, params string[] assemblyPaths)
            {
                if (_configs == null)
                    _configs = new Dictionary<string, ConfigData>();

                if (assemblyPaths.Length == 0)
                {
                    var path = _runtimeDirectory != null
                        ? $"bin/{name}/{_runtimeDirectory}/{AssemblyName}.dll"
                        : $"bin/{name}/{AssemblyName}.dll";
                    assemblyPaths = new[] { path };
                }

                _configs.Add(name, new ConfigData(name, assemblyPaths));

                return this;
            }

            /// <summary>
            /// Override used when displaying test error messages.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return ProjectName;
            }
        }

        /// <summary>
        /// Nested ConfigData class
        /// </summary>
        public class ConfigData
        {
            public ConfigData(string name, params string[] assemblyPaths)
            {
                Name = name;
                AssemblyPaths = assemblyPaths;
            }
            public string Name { get; }
            public string[] AssemblyPaths { get; }
        }

        protected static string NormalizePath(string path)
        {
            return PathSeparatorLookup.Replace(path, Path.DirectorySeparatorChar.ToString());
        }
    }
}
