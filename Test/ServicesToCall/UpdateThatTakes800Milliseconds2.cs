// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using RunMethodsSequentially;

namespace Test.ServicesToCall
{
    public class UpdateThatTakes800Milliseconds2 : IServiceToCallWhileInLock
    {
        public string GlobalLockName { get; } = nameof(RunMethodsSequentially);

        public async ValueTask RunMethodWhileInLockAsync()
        {
            await Task.Delay(800);
        }
    }
}