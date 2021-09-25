using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace NUnit.Engine.Services.ProjectLoaders
{
    /// <summary>
    /// Load a non-C++ project in the MsBuild format introduced with VS2005
    /// </summary>
    public static class NonSdkProjectHelper
    {
        public static void LoadNonSdkProject(this VSProject project, XmlDocument doc)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

            XmlNodeList propertyGroups = doc.SelectNodes("/msbuild:Project/msbuild:PropertyGroup", namespaceManager);
            if (propertyGroups == null) return;

            XmlElement assemblyNameElement = (XmlElement)doc.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:AssemblyName", namespaceManager);
            string assemblyName = assemblyNameElement == null ? project.Name : assemblyNameElement.InnerText;

            XmlElement outputTypeElement = (XmlElement)doc.SelectSingleNode("/msbuild:Project/msbuild:PropertyGroup/msbuild:OutputType", namespaceManager);
            string outputType = outputTypeElement == null ? "Library" : outputTypeElement.InnerText;

            if (outputType == "Exe" || outputType == "WinExe")
                assemblyName = assemblyName + ".exe";
            else
                assemblyName = assemblyName + ".dll";

            string commonOutputPath = null;
            var explicitOutputPaths = new Dictionary<string, string>();

            foreach (XmlElement propertyGroup in propertyGroups)
            {
                string name = propertyGroup.GetConfigNameFromCondition();

                XmlElement outputPathElement = (XmlElement)propertyGroup.SelectSingleNode("msbuild:OutputPath", namespaceManager);
                string outputPath = null;
                if (outputPathElement != null)
                    outputPath = outputPathElement.InnerText;

                if (name == null)
                {
                    if (outputPathElement != null)
                        commonOutputPath = outputPath;
                    continue;
                }

                if (outputPathElement != null)
                    explicitOutputPaths[name] = outputPath;

                if (outputPath == null)
                    outputPath = explicitOutputPaths.ContainsKey(name) ? explicitOutputPaths[name] : commonOutputPath;

                if (outputPath != null && !project.ConfigNames.Contains(name))
                    project.AddConfig(name, Path.Combine(outputPath.Replace("$(Configuration)", name), assemblyName));
            }
        }
    }
}
