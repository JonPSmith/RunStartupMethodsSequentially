using System;

namespace RunMethodsSequentially
{
    /// <summary>
    /// This allow you to set a order value on a <see cref="IServiceToCallWhileInLock"/> service.
    /// The value you provide will define the order your service will be run in
    /// Negative OrderNum: are run before service without a <see cref="WhatOrderToRunInAttribute"/> starting with the lowest
    /// No OrderNum:       service without a <see cref="WhatOrderToRunInAttribute"/> are considered to have a OrderNum of zero
    /// Positive OrderNum: are run after the service without a <see cref="WhatOrderToRunInAttribute"/> starting with the lowest
    /// For services that have the same OrderNum are run in the order that they are defined to the DI provider.
    /// </summary>
    public class WhatOrderToRunInAttribute : Attribute
    {
        public int OrderNum { get;}

        public WhatOrderToRunInAttribute(int orderNum)
        {
            OrderNum = orderNum;
        }
    }
}
