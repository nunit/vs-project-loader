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
        public static bool TryLoadLegacyProject(this VSProject project, XmlDocument doc)
        {
            XmlNode settingsNode = doc.SelectSingleNode("/VisualStudioProject/*/Build/Settings");
            if (settingsNode == null)
                return false;

            string assemblyName = settingsNode.RequiredAttributeValue("AssemblyName");
            string outputType = settingsNode.RequiredAttributeValue("OutputType");
            if (outputType == null)
                throw new ApplicationException("Missing required attribute 'OutputType'");

            if (outputType == "Exe" || outputType == "WinExe")
                assemblyName = assemblyName + ".exe";
            else
                assemblyName = assemblyName + ".dll";

            XmlNodeList nodes = settingsNode.SelectNodes("Config");
            if (nodes != null)
                foreach (XmlNode configNode in nodes)
                {
                    string name = configNode.RequiredAttributeValue("Name");
                    string outputPath = configNode.RequiredAttributeValue("OutputPath");

                    project.AddConfig(name, Path.Combine(outputPath, assemblyName));
                }

            return true;
        }
    }
}
