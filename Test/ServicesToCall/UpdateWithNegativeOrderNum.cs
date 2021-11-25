// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially;
using Test.EfCore;

namespace Test.ServicesToCall
{
    public class UpdateWithNegativeOrderNum : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; } = -1;

        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            var context = scopedServices.GetRequiredService<TestDbContext>();

            context.Add(new NameDateTime { Name = $"OrderNum = -1", DateTimeUtc = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }
    }
}