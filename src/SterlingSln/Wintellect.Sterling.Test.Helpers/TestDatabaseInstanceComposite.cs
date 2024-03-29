using System;
using System.Collections.Generic;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.Test.Helpers
{
    public class TestDatabaseInstanceComposite : BaseDatabaseInstance
    {        
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "TestDatabaseComposite"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                       {
                           CreateTableDefinition<TestCompositeClass, TestCompositeKeyClass>(k=>
                           new TestCompositeKeyClass(k.Key1, k.Key2, k.Key3, k.Key4))                              
                       };
        }
    }
}
