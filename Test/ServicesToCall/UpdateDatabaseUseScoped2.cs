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
    public class UpdateDatabaseUseScoped2 : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; }

        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            var context = scopedServices.GetRequiredService<TestDbContext>();

            var startTime = DateTime.UtcNow;
            //add a new entry
            context.Add(new NameDateTime
                { Name = nameof(UpdateDatabase1), DateTimeUtc = startTime });

            //add/update the common class
            var commonEntity = await context.CommonNameDateTimes.SingleOrDefaultAsync();
            if (commonEntity == null)
                context.Add(new CommonNameDateTime
                { Name = nameof(UpdateDatabase1), DateTimeUtc = startTime });
            else
            {
                commonEntity.DateTimeUtc = startTime;
            }

            await context.SaveChangesAsync();
        }
    }
}