// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RunMethodsSequentially.LockAndRunCode;
using System.IO;
using System.Threading.Tasks;

namespace RunMethodsSequentially
{
    /// <summary>
    /// This class will test your use of the <see cref="StartupExtensions.RegisterRunMethodsSequentially"/>
    /// by registering to the services and then running the code which would be run on the startup of your application
    /// </summary>
    public class RegisterRunMethodsSequentiallyTester
    {
        /// <summary>
        /// You need to register the <see cref="StartupExtensions.RegisterRunMethodsSequentially"/> with its options
        /// as found in your startup code.
        /// You also need to register any services, such as your application's DbContext, that your startup services need
        /// </summary>
        public ServiceCollection Services { get; } = new ServiceCollection();

        /// <summary>
        /// If you are using the <see cref="StartupExtensions.AddFileSystemLockAndRunMethods"/> then you can use this path to a directory
        /// </summary>
        public string LockFolderPath { get; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Run this to check that your <see cref="StartupExtensions.RegisterRunMethodsSequentially"/> with its options work
        /// </summary>
        /// <returns></returns>
        public async Task RunHostStartupCodeAsync()
        {
            Services.AddLogging();
            var serviceProvider = Services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<RunSequentiallyOptions>();
            if (options.RegisterAsHostedService)
            {
                var lockAndRun = serviceProvider.GetRequiredService<IHostedService>();
                await lockAndRun.StartAsync(default);
            }
            else
            {
                var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();
                await lockAndRun.LockAndLoadAsync();
            }
        }

    }
}
