using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

/// <summary>
/// Load a project in the SDK project format.
/// </summary>
namespace NUnit.Engine.Services.ProjectLoaders
{
    public static class SdkProjectHelper
    {
        private static readonly Regex netFramework = new Regex("^net[1-9]");

        public static bool TryLoadSdkProject(this VSProject project, XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("Project");

            if (root != null && root.Attributes["Sdk"]?.Value != null)
            {
                string[] targetFrameworks =
                    doc.SelectSingleNode("Project/PropertyGroup/TargetFrameworks")?.InnerText?.Split(new[] { ';' });

                // TODO: Not currently handling multiple targets. That's a separate issue.
                // This code only handles use of TargetFrameworks with a single value.
                string targetFramework = targetFrameworks != null && targetFrameworks.Length > 0
                    ? targetFrameworks[0]
                    : doc.SelectSingleNode("Project/PropertyGroup/TargetFramework")?.InnerText;

                XmlNode assemblyNameNode = doc.SelectSingleNode("Project/PropertyGroup/AssemblyName");

                // Even console apps are dll's even if <OutputType> has value 'EXE',
                // if TargetFramework is netcore
                string outputType = "dll";

                if (netFramework.IsMatch(targetFramework))
                {
                    // When targetting standard .Net framework, the default is still dll,
                    // however, OutputType is now honoured.
                    // Also if Sdk = 'Microsoft.NET.Sdk.Web' then OutputType default is exe
                    string sdk = root.Attributes["Sdk"].Value;

                    if (sdk == "Microsoft.NET.Sdk.Web")
                    {
                        outputType = "exe";
                    }
                    else
                    {
                        XmlNode outputTypeNode = doc.SelectSingleNode("Project/PropertyGroup/OutputType");
                        if (outputTypeNode != null && outputTypeNode.InnerText != "Library")
                        {
                            outputType = "exe";
                        }
                    }
                }

                string assemblyName = assemblyNameNode == null ? $"{project.Name}.{outputType}" : $"{assemblyNameNode.InnerText}.{outputType}";

                var appendTargetFrameworkNode = doc.SelectSingleNode("Project/PropertyGroup/AppendTargetFrameworkToOutputPath");
                bool appendTargetFramework = appendTargetFrameworkNode == null || appendTargetFrameworkNode.InnerText.ToLower() == "true";

                XmlNodeList propertyGroups = doc.SelectNodes("/Project/PropertyGroup");

                string commonOutputPath = null;

                foreach (XmlElement propertyGroup in propertyGroups)
                {
                    string configName = propertyGroup.GetConfigNameFromCondition();

                    XmlElement outputPathElement = (XmlElement)propertyGroup.SelectSingleNode("OutputPath");
                    string outputPath = outputPathElement?.InnerText;

                    if (outputPath == null)
                        continue;

                    if (configName == null)
                    {
                        commonOutputPath = outputPath;
                        continue;
                    }

                    if (appendTargetFramework)
                    {
                        var suffix = "/" + targetFramework;
                        if (!outputPath.EndsWith(suffix))
                            outputPath += suffix;
                    }

                    project.AddConfig(configName, Path.Combine(outputPath, assemblyName));
                }

                // TODO: Verify this... What is the expected behavior with and without a Configurtions element?

                // By convention there is a Debug and a Release configuration unless others are explicitly 
                // mentioned in the project file. If we have less than 2 then at least one of those is missing.
                // We cannot tell however if the existing configuration is meant to replace Debug or Release.
                // Therefore we just add what is missing. The one that has been replaced will not be used.
                if (project.ConfigNames.Count < 2)
                {
                    foreach (string configName in new[] { "Debug", "Release" })
                    {
                        if (!project.ConfigNames.Contains(configName))
                        {
                            string outputPath = commonOutputPath != null
                                ? commonOutputPath.Replace("$(Configuration)", configName)
                                : $@"bin\{configName}";
                            if (appendTargetFramework)
                                outputPath += "/" + targetFramework;
                            project.AddConfig(configName, Path.Combine(outputPath, assemblyName));
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}
