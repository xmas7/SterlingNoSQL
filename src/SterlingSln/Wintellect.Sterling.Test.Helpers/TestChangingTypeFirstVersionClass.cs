﻿using System;

namespace Wintellect.Sterling.Test.Helpers
{
    public class TestChangingTypeFirstVersionClass
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string PropertyOne { get; set; }

        public string PropertyTwo { get; set; }

        public static TestChangingTypeFirstVersionClass MakeChangingTypeFirstVersionClass()
        {
            return new TestChangingTypeFirstVersionClass
                       {
                           Key = Guid.NewGuid().ToString(),
                           Name = "Name",
                           PropertyOne = "One",
                           PropertyTwo = "Two"
                       };
        }
    }
}