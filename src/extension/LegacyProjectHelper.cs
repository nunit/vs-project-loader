using System;
using System.Collections.Generic;
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
        public static bool TryLoadProject(VSProject project, XmlDocument doc)
        {
            XmlNode settingsNode = doc.SelectSingleNode("/VisualStudioProject/*/Build/Settings");
            if (settingsNode == null)
                return false;

            string assemblyName = RequiredAttributeValue(settingsNode, "AssemblyName");
            string outputType = RequiredAttributeValue(settingsNode, "OutputType");

            if (outputType == "Exe" || outputType == "WinExe")
                assemblyName = assemblyName + ".exe";
            else
                assemblyName = assemblyName + ".dll";

            XmlNodeList nodes = settingsNode.SelectNodes("Config");
            if (nodes != null)
                foreach (XmlNode configNode in nodes)
                {
                    string name = RequiredAttributeValue(configNode, "Name");
                    string outputPath = RequiredAttributeValue(configNode, "OutputPath");

                    project.AddConfig(name, Path.Combine(outputPath, assemblyName));
                }

            return true;
        }

        private static string SafeAttributeValue(XmlNode node, string attrName)
        {
            XmlNode attrNode = node.Attributes[attrName];
            return attrNode == null ? null : attrNode.Value;
        }

        private static string RequiredAttributeValue(XmlNode node, string name)
        {
            string result = SafeAttributeValue(node, name);
            if (result != null)
                return result;

            throw new ApplicationException("Missing required attribute " + name);
        }
    }
}
