// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace Test.EfCore
{
    public class CommonNameDateTime
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stage { get; set; }
        public DateTime DateTimeUtc { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Stage: {Stage}, DateTimeUtc: {DateTimeUtc:O}";
        }
    }
}