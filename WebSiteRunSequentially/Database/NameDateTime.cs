﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace WebSiteRunSequentially.Database
{
    public class NameDateTime
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTimeUtc { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, DateTimeUtc: {DateTimeUtc:O}";
        }
    }
}