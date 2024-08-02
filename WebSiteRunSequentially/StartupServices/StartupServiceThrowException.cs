using RunMethodsSequentially;
using WebSiteRunSequentially.Database;

namespace WebSiteRunSequentially.StartupServices
{
    public class StartupServiceThrowException : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; }
        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            throw new Exception("This should stop the application.");
        }
    }
}
