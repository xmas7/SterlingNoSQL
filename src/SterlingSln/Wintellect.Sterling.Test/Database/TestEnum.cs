using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{
    public enum TestEnums : short
    {
        Value1,
        Value2,
        Value3
    }

    public class EnumClass
    {
        public int Id { get; set; }
        public TestEnums Value { get; set; }
    }

    public class EnumDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Enum"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<EnumClass, int>(e => e.Id)
                           };
        }
    }

    [Tag("Enum")]
    [Tag("Database")]
    [TestClass]
    public class TestEnum
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<EnumDatabase>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _engine.Dispose();
            _databaseInstance = null;
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
        }

        [TestMethod]
        public void TestEnumSaveAndLoad()
        {
            var test = new EnumClass() { Id = 1, Value = TestEnums.Value2 };
            _databaseInstance.Save(test);
            var actual = _databaseInstance.Load<EnumClass>(1);
            Assert.AreEqual(test.Id, actual.Id, "Failed to load enum: key mismatch.");
            Assert.AreEqual(test.Value, actual.Value, "Failed to load enum: value mismatch.");
        }

        [TestMethod]
        public void TestMultipleEnumSaveAndLoad()
        {
            var test1 = new EnumClass { Id = 1, Value = TestEnums.Value1 };
            var test2 = new EnumClass { Id = 2, Value = TestEnums.Value2 };

            _databaseInstance.Save(test1);
            _databaseInstance.Save(test2);

            var actual1 = _databaseInstance.Load<EnumClass>(1);
            var actual2 = _databaseInstance.Load<EnumClass>(2);

            Assert.AreEqual(test1.Id, actual1.Id, "Failed to load enum: key 1 mismatch.");
            Assert.AreEqual(test1.Value, actual1.Value, "Failed to load enum: value 1 mismatch.");

            Assert.AreEqual(test2.Id, actual2.Id, "Failed to load enum: key 2 mismatch.");
            Assert.AreEqual(test2.Value, actual2.Value, "Failed to load enum: value 2 mismatch.");
        }

    }
}