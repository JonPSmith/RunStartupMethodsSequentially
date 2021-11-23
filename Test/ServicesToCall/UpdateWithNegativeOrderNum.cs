// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using RunMethodsSequentially;
using Test.EfCore;

namespace Test.ServicesToCall
{
    [WhatOrderToRunIn(-1)]
    public class UpdateWithNegativeOrderNum : IServiceToCallWhileInLock
    {
        private readonly TestDbContext _context;

        public UpdateWithNegativeOrderNum(TestDbContext context)
        {
            _context = context;

        }

        public async ValueTask RunMethodWhileInLockAsync()
        {
            _context.Add(new NameDateTime { Name = $"OrderNum = -1", DateTimeUtc = DateTime.UtcNow });
            await _context.SaveChangesAsync();
        }
    }
}