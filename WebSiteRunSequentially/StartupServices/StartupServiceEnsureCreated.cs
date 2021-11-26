using RunMethodsSequentially;
using Test.EfCore;

namespace WebSiteRunSequentially.StartupServices
{
    public class StartupServiceEnsureCreated : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; }
        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            var testDbContext = scopedServices.GetRequiredService<TestDbContext>();
            await testDbContext.Database.EnsureCreatedAsync();
        }
    }
}
