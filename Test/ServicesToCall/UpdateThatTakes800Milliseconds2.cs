// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using RunMethodsSequentially;

namespace Test.ServicesToCall
{
    public class UpdateThatTakes800Milliseconds2 : IStartupServiceToRunSequentially
    {
        public string GlobalLockName { get; } = nameof(RunMethodsSequentially);

        public int OrderNum { get; }

        public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
        {
            await Task.Delay(800);
        }
    }
}