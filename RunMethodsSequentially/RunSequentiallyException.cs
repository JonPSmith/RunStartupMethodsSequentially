// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace RunMethodsSequentially
{
    /// <summary>
    /// This is an Exception specific to this library
    /// </summary>
    public class RunSequentiallyException : Exception
    {
        /// <summary>
        /// Use this to throw an exception if there is a problem
        /// </summary>
        /// <param name="message"></param>
        public RunSequentiallyException(string message)
            : base(message) {}
    }
}