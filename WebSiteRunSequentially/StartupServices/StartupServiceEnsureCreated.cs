using RunMethodsSequentially;
using WebSiteRunSequentially.Database;

namespace WebSiteRunSequentially.StartupServices
{
    public class StartupServiceEnsureCreated : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; }
        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            var testDbContext = scopedServices.GetRequiredService<WebSiteDbContext>();
            await testDbContext.Database.EnsureCreatedAsync();
        }
    }
}
