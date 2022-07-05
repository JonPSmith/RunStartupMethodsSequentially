// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This checks that the given filepath leads to a FileSystem directory
    /// </summary>
    public class FileSystemDoesDirectoryExist : IPreLockTest
    {
        private readonly string _directoryFilePath;

        /// <summary>
        /// Ctor - get the FilePath to the FileSystem directory
        /// </summary>
        /// <param name="directoryFilePath"></param>
        public FileSystemDoesDirectoryExist(string directoryFilePath)
        {
            _directoryFilePath = directoryFilePath;
        }

        /// <summary>
        /// Returns true if the FileSystem directory exists
        /// </summary>
        /// <returns></returns>
        public ValueTask<bool> CheckLockResourceExistsAsync()
        {
            return new ValueTask<bool>(Directory.Exists(_directoryFilePath));
        }

        /// <summary>
        /// Returns true if the FileSystem directory exists
        /// </summary>
        /// <returns></returns>
        public bool CheckLockResourceExists()
        {
            return Directory.Exists(_directoryFilePath);
        }
    }
}