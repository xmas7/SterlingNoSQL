#if SILVERLIGHT
using Microsoft.Silverlight.Testing;
#endif 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Test.Helpers;
using System.Linq;

namespace Wintellect.Sterling.Test.Database
{
#if SILVERLIGHT 
    [Tag("List")]
#endif
    [TestClass]
    public class TestLists
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [TestMethod]
        public void TestNullList()
        {
            var expected = TestListModel.MakeTestListModel(false);
            expected.Children = null;
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNull(actual.Children, "Save/load failed: list should be null.");            
        }

        [TestMethod]
        public void TestEmptyList()
        {
            var expected = TestListModel.MakeTestListModel(false);
            expected.Children.Clear();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(0, actual.Children.Count, "Save/load failed: list size mismatch.");            
        }

        [TestMethod]
        public void TestList()
        {
            var expected = TestListModel.MakeTestListModel(false);
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(expected.Children.Count, actual.Children.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Children.Count; x++)
            {
                Assert.AreEqual(expected.Children[x].Key, actual.Children[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.Children[x].Data, actual.Children[x].Data, "Save/load failed: data mismatch.");                
            }
        }

        [TestMethod]
        public void TestListWithNull()
        {
            var expected = TestListModel.MakeTestListModel(true);
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(expected.Children.Count(x => x != null), actual.Children.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Children.Count(y => y != null); x++)
            {
                Assert.AreEqual(expected.Children[x].Key, actual.Children[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.Children[x].Data, actual.Children[x].Data, "Save/load failed: data mismatch.");
            }
        }

        [TestMethod]
        public void TestModelAsList()
        {
            var expected = TestModelAsListModel.MakeTestModelAsList();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestModelAsListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.Id, actual.Id, "Save/load failed: key mismatch.");
            Assert.AreEqual(expected.Count, actual.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Count; x++)
            {
                Assert.AreEqual(expected[x].Key, actual[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected[x].Data, actual[x].Data, "Save/load failed: data mismatch.");
            }
        }

        [TestMethod]
        public void TestModelAsListWithParentReference()
        {
            var expected = TestModelAsListModel.MakeTestModelAsListWithParentReference();
            var key = _databaseInstance.Save(expected);
           
            var actual = _databaseInstance.Load<TestModelAsListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.Id, actual.Id, "Save/load failed: key mismatch.");
            Assert.AreEqual(expected.Count, actual.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Count; x++)
            {
                Assert.AreEqual(expected[x].Key, actual[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected[x].Data, actual[x].Data, "Save/load failed: data mismatch.");
                Assert.AreEqual(expected, expected[x].Parent, "Parent doesn't match");
            }
        }

    }
}