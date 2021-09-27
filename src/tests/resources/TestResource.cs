// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;

namespace NUnit.Engine.Tests.resources
{
    // We use this derived class so that the resources
    // may be found based on its namespace.
    public class TestResource : TempResourceFile
    {
        public TestResource(string name)
            : base(typeof(TestResource), name)
        {
        }

        public TestResource(string name, string filePath)
            : base(typeof(TestResource), name, filePath)
        {
        }
    }
}
