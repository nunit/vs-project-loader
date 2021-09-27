// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Xml;

namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}

namespace NUnit.Engine.Services.ProjectLoaders
{
    public static class XmlExtensions
    {
        public static string RequiredAttributeValue(this XmlNode node, string name)
        {
            string result = node.Attributes[name]?.Value;
            
            if (result == null)
                throw new Exception($"Missing required attribute '{name}'");

            return result;
        }

        public static string GetConfigNameFromCondition(this XmlElement configNode)
        {
            string configName = null;
            XmlAttribute conditionAttribute = configNode.Attributes["Condition"];
            if (conditionAttribute != null)
            {
                string condition = conditionAttribute.Value;
                if (condition.IndexOf("$(Configuration)") >= 0)
                {
                    int start = condition.IndexOf("==");
                    if (start >= 0)
                    {
                        configName = condition.Substring(start + 2).Trim(new char[] { ' ', '\'' });
                        int bar = configName.IndexOf('|');
                        if (bar > 0)
                            configName = configName.Substring(0, bar);
                    }
                }
            }
            return configName;
        }
    }
}
