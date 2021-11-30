// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RunMethodsSequentially;
using RunMethodsSequentially.LockAndRunCode;
using Test.EfCore;
using TestSupport.EfHelpers;
using TestSupport.Helpers;

namespace Test.Helpers
{
    public static class RegisterRunHelpers
    {
        public static ServiceCollection SetupSqlServerRunMethodsSequentially(this TestDbContext context,
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

            return services;
        }

        public static ServiceCollection SetupPostgreSqlRunMethodsSequentially(this TestDbContext context,
            Action<RunSequentiallyOptions> optionsAction = null)
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(dbOptions =>
                dbOptions.UseNpgsql(context.Database.GetConnectionString()));
            var options = services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                options.AddPostgreSqlLockAndRunMethods(context.Database.GetConnectionString());
            });
            optionsAction?.Invoke(options);

            return services;
        }

        public static ServiceCollection SetupNoLockRunMethodsSequentially(this TestDbContext context,
            Action<RunSequentiallyOptions> optionsAction = null)
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(dbOptions =>
                dbOptions.UseSqlServer(context.Database.GetConnectionString()));
            var options = services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                options.AddRunMethodsWithoutLock();
            });
            optionsAction?.Invoke(options);

            return services;
        }

        public static ServiceCollection SetupFileSystemLockMethodsSequentially(this TestDbContext context,
            Action<RunSequentiallyOptions> optionsAction = null)
        {
            var services = new ServiceCollection();
            services.AddDbContext<TestDbContext>(dbOptions =>
                dbOptions.UseSqlServer(context.Database.GetConnectionString()));
            var options = services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                options.AddFileSystemLockAndRunMethods(TestData.GetTestDataDir());
            });
            optionsAction?.Invoke(options);

            return services;
        }
    }
}