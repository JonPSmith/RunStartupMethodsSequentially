using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RunMethodsSequentially.LockAndRunCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSupport.EfHelpers;

namespace Test.Helpers
{
    public class RegisterTestLogger
    {
        public List<LogOutput> Logs = new List<LogOutput>();

        public RegisterTestLogger(ServiceCollection services)
        {
            services.AddSingleton<ILogger<GetLockAndThenRunServices>>(
                new Logger<GetLockAndThenRunServices>(new LoggerFactory(new[] { new MyLoggerProviderActionOut(Logs.Add) })));
        }

    }
}
