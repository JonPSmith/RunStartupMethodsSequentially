// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace RunMethodsSequentially.TestHelpers
{
    public class LocalLogOutput
    {
        internal LocalLogOutput(LogLevel logLevel,
            EventId eventId, string message)
        {
            LogLevel = logLevel;
            EventId = eventId;
            Message = message;
        }

        /// <summary>
        /// The logLevel of this log
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// The logging EventId - should be string for EF Core logs
        /// </summary>
        public EventId EventId { get; }

        /// <summary>
        /// The message in the log
        /// </summary>
        public string Message { get; }
    }
}
