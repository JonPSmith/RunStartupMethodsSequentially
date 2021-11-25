// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RunMethodsSequentially;
using Test.EfCore;

namespace Test.ServicesToCall
{
    public class UpdateDatabase2 : IStartupServiceToRunSequentially
    {
        private readonly TestDbContext _context;

        public UpdateDatabase2(TestDbContext context)
        {
            _context = context;
        }
        public int OrderNum { get; }

        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            var startTime = DateTime.UtcNow;
            //add a new entry
            _context.Add(new NameDateTime
                { Name = nameof(UpdateDatabase1), DateTimeUtc = startTime });

            //add/update the common class
            var commonEntity = await _context.CommonNameDateTimes.SingleOrDefaultAsync();
            if (commonEntity == null)
                _context.Add(new CommonNameDateTime
                { Name = nameof(UpdateDatabase1), DateTimeUtc = startTime });
            else
            {
                commonEntity.DateTimeUtc = startTime;
            }

            await _context.SaveChangesAsync();
        }
    }
}