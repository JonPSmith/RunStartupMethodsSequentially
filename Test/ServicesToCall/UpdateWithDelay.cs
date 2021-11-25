// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially;
using Test.EfCore;

namespace Test.ServicesToCall
{
    public class UpdateWithDelay : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; }

        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            var context = scopedServices.GetRequiredService<TestDbContext>();

            var commonEntity = await context.CommonNameDateTimes.SingleAsync();

            context.Add(new NameDateTime { Name = "Read", DateTimeUtc = commonEntity.DateTimeUtc });
            await context.SaveChangesAsync();

            await Task.Delay(100);
            commonEntity.DateTimeUtc = DateTime.UtcNow;
            commonEntity.Name = Guid.NewGuid().ToString();
            context.Add(new NameDateTime { Name = "Write", DateTimeUtc = commonEntity.DateTimeUtc });

            await context.SaveChangesAsync();
        }
    }
}