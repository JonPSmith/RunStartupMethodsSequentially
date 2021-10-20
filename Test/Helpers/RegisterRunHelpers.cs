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
        public static IGetLockAndThenRunServices SetupSqlServerRunMethodsSequentially(this TestDbContext context,
            Action<RunSequentiallyOptions> optionsAction = null)
        {
            var services = new ServiceCollection();
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

        public static IGetLockAndThenRunServices SetupPostGreSqlRunMethodsSequentially(this TestDbContext context,
            Action<RunSequentiallyOptions> optionsAction = null)
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(dbOptions =>
                dbOptions.UseNpgsql(context.Database.GetConnectionString()));
            var options = services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                options.AddPostGreSqlLockAndRunMethods(context.Database.GetConnectionString());
            });
            optionsAction?.Invoke(options);

            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();
            return lockAndRun;
        }
    }
}