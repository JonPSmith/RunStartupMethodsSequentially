// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using RunMethodsSequentially;
using Test.EfCore;

namespace Test.ServicesToCall
{
    public class SqlServerEnsureCreatedDatabaseOnly : IStartupServiceToRunSequentially
    {
        private readonly TestDbContext _context;

        public SqlServerEnsureCreatedDatabaseOnly(TestDbContext context)
        {
            _context = context;
        }

        public int OrderNum { get; }

        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            await _context.Database.EnsureCreatedAsync();
        }
    }
}