// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RunMethodsSequentially;
using Test.EfCore;

namespace Test.ServicesToCall
{
    public class UpdateWithDelay : IServiceToCallWhileInLock
    {
        private readonly TestDbContext _context;

        public UpdateWithDelay(TestDbContext context)
        {
            _context = context;

        }

        public async ValueTask RunMethodWhileInLockAsync()
        {
            var commonEntity = await _context.CommonNameDateTimes.SingleAsync();

            _context.Add(new NameDateTime { Name = "Read", DateTimeUtc = commonEntity.DateTimeUtc });
            await _context.SaveChangesAsync();

            await Task.Delay(100);
            commonEntity.DateTimeUtc = DateTime.UtcNow;
            commonEntity.Name = Guid.NewGuid().ToString();
            _context.Add(new NameDateTime { Name = "Write", DateTimeUtc = commonEntity.DateTimeUtc });

            await _context.SaveChangesAsync();
        }
    }
}