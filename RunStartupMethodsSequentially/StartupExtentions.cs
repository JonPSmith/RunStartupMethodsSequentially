// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace RunStartupMethodsSequentially
{
    public static class StartupExtensions
    {
        public static RunSequentiallyOptions RegisterRunMethodsSequentially(this IServiceCollection services,
            Action<RunSequentiallyOptions> options = null)
        {
            var lockOptions = new RunSequentiallyOptions(services);
            options?.Invoke(lockOptions);


            return lockOptions;
        }
    }
}