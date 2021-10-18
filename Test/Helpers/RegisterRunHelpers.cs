// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RunMethodsSequentially;
using RunMethodsSequentially.LockAndRunCode;
using Test.EfCore;

namespace Test.Helpers
{
    public static class RegisterRunHelpers
    {
        public static IGetLockAndThenRunServices SetupRunMethodsSequentially(this TestDbContext context,
            Action<RunSequentiallyOptions> optionsAction = null)
        {
            var services = new ServiceCollection();
            services.AddLogging(config => config.AddConsole());
            services.AddDbContext<TestDbContext>(dbOptions =>
                dbOptions.UseSqlServer(context.Database.GetConnectionString()));
            var options = services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
            });
            optionsAction?.Invoke(options);

            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();
            return lockAndRun;
        }
    }
}