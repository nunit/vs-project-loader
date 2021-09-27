// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Xml;

namespace NUnit.Engine.Services.ProjectLoaders
{
    /// <summary>
    /// Load a project in the legacy VS2003 format. Note that this helper is not 
    /// used for C++ projects using the same format, because the details differ.
    /// </summary>
    public static class LegacyProjectHelper
    {
        public static void LoadLegacyProject(this VSProject project, XmlDocument doc)
        {
            XmlNode settingsNode = doc.SelectSingleNode("/VisualStudioProject/*/Build/Settings");
            if (settingsNode == null)
                throw new InvalidOperationException("Invalid format for Legacy project");

            string assemblyName = settingsNode.RequiredAttributeValue("AssemblyName");
            string outputType = settingsNode.RequiredAttributeValue("OutputType");

            if (outputType == "Exe" || outputType == "WinExe")
                assemblyName = assemblyName + ".exe";
            else
                assemblyName = assemblyName + ".dll";

            XmlNodeList configNodes = settingsNode.SelectNodes("Config");
            if (configNodes == null)
                throw new InvalidOperationException("No configurations found in project");

            foreach (XmlNode configNode in configNodes)
            {
                string name = configNode.RequiredAttributeValue("Name");
                string outputPath = configNode.RequiredAttributeValue("OutputPath");

                project.AddConfig(name, Path.Combine(outputPath, assemblyName));
            }
        }
    }
}
