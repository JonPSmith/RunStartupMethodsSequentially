// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Test.Helpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestSqlServerHelpers
    {

        [Fact]
        public void TestCheckCreateDelete()
        {
            //SETUP
            var connectionString = this.GetUniqueDatabaseConnectionString();
            if (connectionString.DoesDatabaseExist())
                connectionString.DeleteDatabase();
            connectionString.DoesDatabaseExist().ShouldBeFalse();

            //ATTEMPT - VERIFY
            connectionString.CreateDatabase();
            connectionString.DoesDatabaseExist().ShouldBeTrue();
            connectionString.DeleteDatabase();
            connectionString.DoesDatabaseExist().ShouldBeFalse();
        }
    }
}