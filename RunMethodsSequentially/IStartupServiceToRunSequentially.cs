// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace RunMethodsSequentially
{
    /// <summary>
    /// This defines the service that you want run within a locked state
    /// </summary>
    public interface IStartupServiceToRunSequentially
    {
        /// <summary>
        /// This defines the order in which your startup services are run.
        /// Note that startup services with the same <see cref="OrderNum"/> will 
        /// be run in the order that they were registered with the <see cref="StartupExtensions.RegisterServiceToRunInJob"/> 
        /// </summary>
        public int OrderNum { get; }

        /// <summary>
        /// This method will be called within the RunMethodsSequentially JobRunner
        /// </summary>
        /// <param name="scopedServices">The RunMethodsSequentially JobRunner will provide a scoped service provider,
        /// which allows you to obtain services more efficiently, but be aware the scoped service is used by every 
        /// startup services you register. But be aware that sharing the scoped service this might cause issues 
        /// whn using EF Core if 
        /// </param>
        /// <returns></returns>
        ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices);
    }
}