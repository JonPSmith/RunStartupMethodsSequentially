// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RunMethodsSequentially.LockAndRunCode;

/// <summary>
/// provides the hosted service that runs the registered <see cref="IStartupServiceToRunSequentially"/>
/// services when the ASP.NET Core application starts
/// </summary>
public class GetLockAndThenRunHostedService : IHostedService
{
    private readonly IGetLockAndThenRunServices _service;

    /// <summary>
    /// Ctor - gets the <see cref="IServiceProvider"/>
    /// </summary>
    /// <param name="serviceProvider"></param>
    public GetLockAndThenRunHostedService(IServiceProvider serviceProvider)
    {
        _service = new GetLockAndThenRunServices(serviceProvider);
    }

    /// <summary>
    /// This obtains a lock and then runs all the registered <see cref="IStartupServiceToRunSequentially"/> services
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var success = await _service.LockAndLoadAsync();
    }

    /// <summary>
    /// Not used
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}