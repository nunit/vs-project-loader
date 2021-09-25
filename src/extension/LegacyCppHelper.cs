using System.IO;
using System.Xml;

namespace NUnit.Engine.Services.ProjectLoaders
{
    /// <summary>
    /// Load a C++ project in the legacy format, which was used for C++
    /// much longer than it was used for the other languages supported.
    /// </summary>
    public static class LegacyCppHelper
    {
        public static void LoadLegacyCppProject(this VSProject project, XmlDocument doc)
        {
            string[] extensionsByConfigType = { "", ".exe", ".dll", ".lib", "" };

            // TODO: This is all very hacked up... replace it.
            foreach (XmlNode configNode in doc.SelectNodes("/VisualStudioProject/Configurations/Configuration"))
            {
                string name = configNode.RequiredAttributeValue("Name");
                int config_type = System.Convert.ToInt32(configNode.RequiredAttributeValue("ConfigurationType"));
                string dirName = name;
                int bar = dirName.IndexOf('|');
                if (bar >= 0)
                    dirName = dirName.Substring(0, bar);
                string outputPath = configNode.RequiredAttributeValue("OutputDirectory");
                outputPath = outputPath.Replace("$(SolutionDir)", Path.GetFullPath(Path.GetDirectoryName(project.ProjectPath)) + Path.DirectorySeparatorChar);
                outputPath = outputPath.Replace("$(ConfigurationName)", dirName);

                XmlNode toolNode = configNode.SelectSingleNode("Tool[@Name='VCLinkerTool']");
                string assemblyName = null;
                if (toolNode != null)
                {
                    assemblyName = toolNode.Attributes["OutputFile"]?.Value;
                    if (assemblyName != null)
                        assemblyName = Path.GetFileName(assemblyName);
                    else
                        assemblyName = Path.GetFileNameWithoutExtension(project.ProjectPath) + extensionsByConfigType[config_type];
                }
                else
                {
                    toolNode = configNode.SelectSingleNode("Tool[@Name='VCNMakeTool']");
                    if (toolNode != null)
                        assemblyName = Path.GetFileName(toolNode.RequiredAttributeValue("Output"));
                }

                assemblyName = assemblyName.Replace("$(OutDir)", outputPath);
                assemblyName = assemblyName.Replace("$(ProjectName)", project.Name);

                project.AddConfig(name, Path.Combine(outputPath, assemblyName));
            }
        }
    }
}
