// ***********************************************************************
// Copyright (c) Charlie Poole and contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

namespace NUnit.Engine.Tests
{
    using System;
    using System.IO;

    public class TempResourceFile : IDisposable
    {
        static readonly string TEMP_PATH = System.IO.Path.GetTempPath();

        public string Path { get; private set; }

        public bool IsRelativeToTempPath  { get; private set; }

        public TempResourceFile(Type type, string name) : this(type, name, null) {}

        public TempResourceFile(Type type, string name, string filePath)
        {
            if (filePath == null)
                filePath = name;

            IsRelativeToTempPath = !System.IO.Path.IsPathRooted(filePath);
            if (IsRelativeToTempPath)
                filePath = System.IO.Path.Combine(TEMP_PATH, filePath);

            Path = filePath;

            Stream stream = type.Assembly.GetManifestResourceStream(type, name);
            byte[] buffer = new byte[(int)stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            string dir = System.IO.Path.GetDirectoryName(Path);
            if(dir != null && dir.Length != 0)
            {
                Directory.CreateDirectory(dir);
            }

            using(FileStream fileStream = new FileStream(Path, FileMode.Create))
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        public void Dispose()
        {
            File.Delete(Path);
            
            string path = Path;
            while(true)
            {
                path = System.IO.Path.GetDirectoryName(path);
                if(path == null || path.Length == 0)
                    break;
                if(IsRelativeToTempPath && path.Length <= TEMP_PATH.Length)
                    break;
                if(Directory.GetFiles(path).Length > 0)
                    break;

                Directory.Delete(path);
            }
        }
    }
}
