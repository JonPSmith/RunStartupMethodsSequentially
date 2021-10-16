// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode
{
    public class FileSystemDoesDirectoryExist : IPreLockTest
    {
        private readonly string _directoryFilePath;

        public FileSystemDoesDirectoryExist(string directoryFilePath)
        {
            _directoryFilePath = directoryFilePath;
        }

        public ValueTask<bool> CheckLockResourceExistsAsync()
        {
            return new ValueTask<bool>(Directory.Exists(_directoryFilePath));
        }
    }
}