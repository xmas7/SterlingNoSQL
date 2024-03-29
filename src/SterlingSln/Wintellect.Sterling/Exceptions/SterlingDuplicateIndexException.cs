﻿using System;

namespace Wintellect.Sterling.Exceptions
{
    public class SterlingDuplicateIndexException : SterlingException 
    {
        public SterlingDuplicateIndexException(string indexName, Type type, string databaseName) : 
        base (string.Format(Exceptions.SterlingDuplicateIndexException, indexName, type.FullName, databaseName))
        {
            
        }        
    }
}
