using RunMethodsSequentially;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    public class DoNothingStartupService : IStartupServiceToRunSequentially
    {
        public int OrderNum { get; }
        public ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            return ValueTask.CompletedTask;
        }
    }
}
