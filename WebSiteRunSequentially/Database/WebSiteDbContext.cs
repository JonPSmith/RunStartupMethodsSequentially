// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace WebSiteRunSequentially.Database
{
    public class WebSiteDbContext : DbContext
    {
        public WebSiteDbContext(DbContextOptions<WebSiteDbContext> options)
            : base(options) { }

        public DbSet<NameDateTime> NameDateTimes { get; set; }
        public DbSet<CommonNameDateTime> CommonNameDateTimes { get; set; }
    }
}