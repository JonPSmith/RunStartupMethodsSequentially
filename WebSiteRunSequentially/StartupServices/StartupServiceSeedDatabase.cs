using Microsoft.EntityFrameworkCore;
using RunMethodsSequentially;
using Test.EfCore;

namespace WebSiteRunSequentially.StartupServices
{
    public class StartupServiceSeedDatabase : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; }
        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            var context = scopedServices.GetRequiredService<TestDbContext>();

            var startTime = DateTime.UtcNow;

            var commonEntity = await context.CommonNameDateTimes.SingleOrDefaultAsync();
            if (commonEntity == null)
            {
                context.Add(new CommonNameDateTime{ Name = "new", Stage = 1, DateTimeUtc = startTime });
                context.RemoveRange(context.NameDateTimes);
                context.Add(new NameDateTime { Name = "No common entity found. Deleted old logs, created a new common and set its Stage to 1", DateTimeUtc = startTime });
            }           
            else if (commonEntity.DateTimeUtc < startTime.AddSeconds(-20))
            {
                commonEntity.DateTimeUtc = startTime;
                commonEntity.Stage = 1;
                var timeDiff = startTime - commonEntity.DateTimeUtc;
                context.RemoveRange(context.NameDateTimes);
                context.Add(new NameDateTime { Name = $"Common entity found, but was {timeDiff:c} old. Deleted old logs,updated common to Stage to 1", DateTimeUtc = startTime });
            }
            else
            {
                commonEntity.DateTimeUtc = startTime;
                commonEntity.Stage =+ 1;
                context.Add(new NameDateTime { Name = $"Common entity found. Updated common to Stage to {commonEntity.Stage}, DateTimeUtc = startTime" });
            }

            await context.SaveChangesAsync();
        }
    }
}
