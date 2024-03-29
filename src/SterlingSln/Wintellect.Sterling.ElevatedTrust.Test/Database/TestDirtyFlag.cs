using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.ElevatedTrust.Test.Database
{
    public class DirtyDatabase : BaseDatabaseInstance
    {       

        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "DirtyDatabase"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<TestListModel, int>(t=>t.ID),
                               CreateTableDefinition<TestModel, int>(t=>t.Key)
                               .WithDirtyFlag<TestModel,int>(o=>TestDirtyFlag.IsTestDirty(o))
                           };
        }
    }

    [Tag("Dirty")]
    [Tag("Database")]
    [TestClass]
    public class TestDirtyFlag
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        public static Predicate<TestModel> IsTestDirty = model => true;

        [TestInitialize]
        public void TestInit()
        {
            FileSystemHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<DirtyDatabase>(new ElevatedTrustDriver());
            _databaseInstance.Purge();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [TestMethod]
        public void TestDirtyFlagFalse()
        {
            var expected = TestListModel.MakeTestListModel(false);

            // first save is to generate the keys
            var key = _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestListModel>(key);

            foreach(var model in actual.Children)
            {
                model.ResetAccess();
            }

            // set so it is always dirty
            IsTestDirty = model => true;

            // now check that all were accessed
            _databaseInstance.Save(actual);

            var accessed = (from t in actual.Children where !t.Accessed select 1).Any();

            Assert.IsFalse(accessed, "Dirty flag on save failed: some children were not accessed.");
        }

        [TestMethod]
        public void TestDirtyFlagTrue()
        {
            var expected = TestListModel.MakeTestListModel(false);

            // first save is to generate the keys
            var key = _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestListModel>(key);

            foreach (var model in actual.Children)
            {
                model.ResetAccess();
            }

            // set so it is never dirty
            IsTestDirty = model => false;

            // now check that none were accessed
            _databaseInstance.Save(actual);

            var accessed = (from t in actual.Children where t.Accessed select 1).Any();

            Assert.IsFalse(accessed, "Dirty flag on save failed: some children were accessed.");
        }

    }
}