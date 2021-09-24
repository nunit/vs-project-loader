using System;
using System.IO;
using System.Xml;

namespace NUnit.Engine.Services.ProjectLoaders
{
    public static class LegacyCppHelper
    {
        /// <summary>
        /// Load a C++ project in the legacy format, which was used for C++
        /// much longer than it was used for the other languages supported.
        /// </summary>
        public static void LoadProject(VSProject project, XmlDocument doc)
        {
            string[] extensionsByConfigType = { "", ".exe", ".dll", ".lib", "" };

            // TODO: This is all very hacked up... replace it.
            foreach (XmlNode configNode in doc.SelectNodes("/VisualStudioProject/Configurations/Configuration"))
            {
                string name = RequiredAttributeValue(configNode, "Name");
                int config_type = System.Convert.ToInt32(RequiredAttributeValue(configNode, "ConfigurationType"));
                string dirName = name;
                int bar = dirName.IndexOf('|');
                if (bar >= 0)
                    dirName = dirName.Substring(0, bar);
                string outputPath = RequiredAttributeValue(configNode, "OutputDirectory");
                outputPath = outputPath.Replace("$(SolutionDir)", Path.GetFullPath(Path.GetDirectoryName(project.ProjectPath)) + Path.DirectorySeparatorChar);
                outputPath = outputPath.Replace("$(ConfigurationName)", dirName);

                XmlNode toolNode = configNode.SelectSingleNode("Tool[@Name='VCLinkerTool']");
                string assemblyName = null;
                if (toolNode != null)
                {
                    assemblyName = SafeAttributeValue(toolNode, "OutputFile");
                    if (assemblyName != null)
                        assemblyName = Path.GetFileName(assemblyName);
                    else
                        assemblyName = Path.GetFileNameWithoutExtension(project.ProjectPath) + extensionsByConfigType[config_type];
                }
                else
                {
                    toolNode = configNode.SelectSingleNode("Tool[@Name='VCNMakeTool']");
                    if (toolNode != null)
                        assemblyName = Path.GetFileName(RequiredAttributeValue(toolNode, "Output"));
                }

                assemblyName = assemblyName.Replace("$(OutDir)", outputPath);
                assemblyName = assemblyName.Replace("$(ProjectName)", project.Name);

                project.AddConfig(name, Path.Combine(outputPath, assemblyName));
            }
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
