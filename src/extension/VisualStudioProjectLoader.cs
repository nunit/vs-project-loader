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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Engine.Extensibility;

namespace NUnit.Engine.Services.ProjectLoaders
{
    /// <summary>
    /// Summary description for VSProjectLoader.
    /// </summary>
    [Extension]
    [ExtensionProperty("FileExtension", ".sln")]
    [ExtensionProperty("FileExtension", ".csproj")]
    [ExtensionProperty("FileExtension", ".vbproj")]
    [ExtensionProperty("FileExtension", ".vjsproj")]
    [ExtensionProperty("FileExtension", ".vcproj")]
    [ExtensionProperty("FileExtension", ".fsproj")]
    public class VisualStudioProjectLoader : IProjectLoader
    {
        private const string SOLUTION_EXTENSION = ".sln";
        private static readonly string[] PROJECT_EXTENSIONS = { ".csproj", ".vbproj", ".vjsproj", ".vcproj", ".fsproj" };

        IDictionary<string, VSProject> _projectLookup = new Dictionary<string, VSProject>();
        //IDictionary<string, SolutionConfig> _configs = new Dictionary<string, SolutionConfig>();
        IDictionary<string, List<string>> _configs = new Dictionary<string, List<string>>();

        #region IProjectLoader Members

        public bool CanLoadFrom(string path)
        {
            return IsProjectFile(path) || IsSolutionFile(path);
        }

        public IProject LoadFrom(string path)
        {
            if (IsProjectFile(path))
                return LoadVSProject(path);

            if (IsSolutionFile(path))
                return LoadVSSolution(path);

            throw new ArgumentException(
                $"Invalid project file type: {Path.GetFileName(path)}");
        }

        #endregion

        #region Helper Methods

        private static bool IsProjectFile(string path)
        {
            if (!IsValidFilePath(path))
                return false;

            string extension = Path.GetExtension(path);

            foreach (string validExtension in PROJECT_EXTENSIONS)
                if (extension == validExtension)
                    return true;

            return false;
        }

        private static bool IsSolutionFile(string path)
        {
            return IsValidFilePath(path) && Path.GetExtension(path) == SOLUTION_EXTENSION;
        }

        private static bool IsValidFilePath(string path)
        {
            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                return false;

            if (path.ToLower().IndexOf("http:") >= 0)
                return false;

            return true;
        }

        const string BUILD_MARKER = ".Build.0 =";

        private VSProject LoadVSProject(string path)
        {
            var project = new VSProject(path);
            var doc = CreateProjectDocument(path);
            var ext = Path.GetExtension(path);
            var projFormat = GetProjectFormat(doc);

            if (!FormatIsSupportedForExtension(projFormat, ext))
                throw new Exception($"The {projFormat} format is not supported for {ext} files!");

            switch(projFormat)
            {
                case ProjectFormat.Sdk:
                    project.LoadSdkProject(doc);
                    break;

                case ProjectFormat.NonSdk:
                    project.LoadNonSdkProject(doc);
                    break;

                case ProjectFormat.Legacy:
                    if (ext == ".vcproj")
                        project.LoadLegacyCppProject(doc);
                    else
                        project.LoadLegacyProject(doc);
                    break;
            }

            return project;
        }

        private XmlDocument CreateProjectDocument(string projectPath)
        {
            try
            {
                using (StreamReader rdr = new StreamReader(projectPath, System.Text.Encoding.UTF8))
                {
                    var doc = new XmlDocument();
                    doc.Load(rdr);
                    return doc;
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                    $"Invalid project file format: {Path.GetFileName(projectPath)}", e);
            }
        }

        private enum ProjectFormat
        {
            Unknown,
            Sdk,
            NonSdk,
            Legacy
        }

        private ProjectFormat GetProjectFormat(XmlDocument doc)
        {
            var docElement = doc.DocumentElement;

            switch (docElement.Name)
            {
                case "Project":
                    return docElement.Attributes["Sdk"]?.Value != null
                        ? ProjectFormat.Sdk
                        : ProjectFormat.NonSdk;

                case "VisualStudioProject":
                    return ProjectFormat.Legacy;

                default:
                    return ProjectFormat.Unknown;
            }
        }

        private bool FormatIsSupportedForExtension(ProjectFormat projFormat, string ext)
        {
            // NOTE: We rely on the fact that the extensions and formats have been pre-checked
            switch (ext)
            {
                case ".csproj":
                case ".fsproj":
                case ".vbproj":
                    return true; // All formats are supported

                case ".vjsproj":
                    return projFormat != ProjectFormat.Sdk;

                case ".vcproj":
                    return projFormat == ProjectFormat.Legacy;

                default:
                    return false;
            }
        }

        private VSSolution LoadVSSolution(string path)
        {
            var solution = new VSSolution(path);

            string solutionDirectory = Path.GetDirectoryName(path);
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                if (line.StartsWith("Project("))
                    ProcessProjectLine(solutionDirectory, line);
                else if (line.IndexOf(BUILD_MARKER) >= 0)
                    ProcessBuildLine(line);
            }

            foreach (string configName in _configs.Keys)
            {
                var assemblies = _configs[configName].ToArray();
                solution.AddConfig(configName, assemblies);
            }

            return solution;
        }

        private void ProcessProjectLine(string solutionDirectory, string line)
        {
            char[] DELIMS = { '=', ',' };
            char[] TRIM_CHARS = { ' ', '"' };
            Regex PathSeparatorLookup = new Regex(@"[/\\]");

            string[] parts = line.Split(DELIMS);
            string vsProjectPath = PathSeparatorLookup.Replace(parts[2].Trim(TRIM_CHARS), Path.DirectorySeparatorChar.ToString());
            string vsProjectGuid = parts[3].Trim(TRIM_CHARS);

            if (IsProjectFile(vsProjectPath))
            {
                var vsProject = LoadVSProject(Path.Combine(solutionDirectory, vsProjectPath));

                _projectLookup[vsProjectGuid] = vsProject;
            }
        }

        private void ProcessBuildLine(string line)
        {
            line = line.Trim();
            int endBrace = line.IndexOf('}');

            string vsProjectGuid = line.Substring(0, endBrace + 1);
            VSProject vsProject;
            if (_projectLookup.TryGetValue(vsProjectGuid, out vsProject))
            {
                line = line.Substring(endBrace + 2);

                int split = line.IndexOf(BUILD_MARKER) + 1;
                string solutionConfig = line.Substring(0, split - 1);
                int bar = solutionConfig.IndexOf('|');
                if (bar >= 0)
                    solutionConfig = solutionConfig.Substring(0, bar);

                string projectConfig = line.Substring(split + BUILD_MARKER.Length);
                if (!vsProject.ConfigNames.Contains(projectConfig))
                {
                    bar = projectConfig.IndexOf('|');
                    if (bar >= 0)
                        projectConfig = projectConfig.Substring(0, bar);
                }

                if (!_configs.Keys.Contains(solutionConfig))
                    _configs[solutionConfig] = new List<string>();


                foreach (var subPackage in vsProject.GetTestPackage(projectConfig).SubPackages)
                    if (!_configs[solutionConfig].Contains(subPackage.FullName))
                        _configs[solutionConfig].Add(subPackage.FullName);

                //if (VSProject.IsProjectFile(vsProjectPath))
                //    project.Add(new VSProject(Path.Combine(solutionDirectory, vsProjectPath)));
            }
        }

        #endregion
    }
}
