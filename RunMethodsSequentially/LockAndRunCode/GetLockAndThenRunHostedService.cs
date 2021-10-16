// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RunMethodsSequentially.LockAndRunCode
{
    public class GetLockAndThenRunHostedService : IHostedService
    {
        private readonly IGetLockAndThenRunServices _service;

        public GetLockAndThenRunHostedService(IServiceProvider serviceProvider)
        {
            _service = new GetLockAndThenRunServices(serviceProvider);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _service.LockAndLoadAsync();
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}