// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using RunMethodsSequentially;
using Test.EfCore;

namespace Test.ServicesToCall
{
    public class UpdateWithNoOrderNum : IServiceToCallWhileInLock
    {
        private readonly TestDbContext _context;

        public UpdateWithNoOrderNum(TestDbContext context)
        {
            _context = context;

        }

        public async ValueTask RunMethodWhileInLockAsync()
        {
            _context.Add(new NameDateTime { Name = $"No OrderNum", DateTimeUtc = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }
    }
}