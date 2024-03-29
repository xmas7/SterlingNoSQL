﻿using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Keys;
using Wintellect.Sterling.Serialization;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.ElevatedTrust.Test.Keys
{
    [Tag("KeyCollection")]
    [TestClass]
    public class TestKeyCollection
    {
        private readonly TestModel[] _models = new[]
                                          {
                                              TestModel.MakeTestModel(), TestModel.MakeTestModel(),
                                              TestModel.MakeTestModel()
                                          };

        private ElevatedTrustDriver _driver;
        private KeyCollection<TestModel, int> _target;
        private readonly ISterlingDatabaseInstance _testDatabase = new TestDatabaseInterfaceInstance();
        private int _testAccessCount;

        /// <summary>
        ///     Fetcher - also flag the fetch
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The model</returns>
        private TestModel _GetTestModelByKey(int key)
        {
            _testAccessCount++;
            return (from t in _models where t.Key.Equals(key) select t).FirstOrDefault();
        }

        [TestInitialize]
        public void TestInit()
        {
            _driver = new ElevatedTrustDriver(_testDatabase.Name, new DefaultSerializer(), SterlingFactory.GetLogger().Log);
            _testAccessCount = 0;            
            _target = new KeyCollection<TestModel, int>(_driver,
                                                        _GetTestModelByKey);
        }       

        [TestMethod]
        public void TestAddKey()
        {
            Assert.IsFalse(_target.IsDirty, "Dirty flag set prematurely");
            Assert.AreEqual(0,_target.NextKey, "Next key is incorrect.");
            _target.AddKey(_models[0].Key);
            Assert.IsTrue(_target.IsDirty, "Dirty flag not set on add.");
            Assert.AreEqual(1, _target.NextKey, "Next key not advanced.");
        }

        [TestMethod]
        public void TestAddDuplicateKey()
        {
            Assert.IsFalse(_target.IsDirty, "Dirty flag set prematurely");
            Assert.AreEqual(0,_target.NextKey, "Next key is incorrect initialized.");
            _target.AddKey(_models[0].Key);
            Assert.IsTrue(_target.IsDirty, "Dirty flag not set on add.");
            Assert.AreEqual(1, _target.NextKey, "Next key not advanced.");
            _target.AddKey(_models[0].Key);
            Assert.AreEqual(1, _target.NextKey, "Next key advanced on duplicate add."); 
            Assert.AreEqual(1, _target.Query.Count(), "Key list count is incorrect.");
        }
        
        [TestMethod]
        public void TestRemoveKey()
        {
            Assert.IsFalse(_target.IsDirty, "Dirty flag set prematurely");
            Assert.AreEqual(0, _target.NextKey, "Next key is incorrect.");
            _target.AddKey(_models[0].Key);
            Assert.IsTrue(_target.IsDirty, "Dirty flag not set on add.");
            Assert.AreEqual(1, _target.NextKey, "Next key not advanced.");       
            _target.RemoveKey(_models[0].Key);
            Assert.AreEqual(0, _target.Query.Count(), "Key was not removed.");
        }

        [TestMethod]
        public void TestQueryable()
        {
            _target.AddKey(_models[0].Key);
            _target.AddKey(_models[1].Key);
            _target.AddKey(_models[2].Key);
            Assert.AreEqual(3, _target.Query.Count(), "Key count is incorrect.");
            Assert.AreEqual(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testKey = (from k in _target.Query where k.Key.Equals(_models[1].Key) select k).FirstOrDefault();
            Assert.IsNotNull(testKey, "Test key not retrieved.");
            Assert.AreEqual(_models[1].Key, testKey.Key, "Key mismatch.");
            Assert.AreEqual(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testModel = testKey.LazyValue.Value; 
            Assert.AreSame(_models[1], testModel, "Model does not match.");
            Assert.AreEqual(1, _testAccessCount, "Lazy loader access count is incorrect.");
            
        }
         
        [TestMethod]
        public void TestSerialization()
        {
            _target.AddKey(_models[0].Key);
            _target.AddKey(_models[1].Key);
            Assert.IsTrue(_target.IsDirty, "Dirty flag not set.");
            _target.Flush();
            Assert.IsFalse(_target.IsDirty, "Dirty flag not reset on flush.");

            var secondTarget = new KeyCollection<TestModel, int>(_driver,
                                                                 _GetTestModelByKey);

            // are we able to grab things?
            Assert.AreEqual(2, secondTarget.Query.Count(), "Key count is incorrect.");
            Assert.AreEqual(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testKey = (from k in secondTarget.Query where k.Key.Equals(_models[1].Key) select k).FirstOrDefault();
            Assert.IsNotNull(testKey, "Test key not retrieved.");
            Assert.AreEqual(_models[1].Key, testKey.Key, "Key mismatch.");
            Assert.AreEqual(0, _testAccessCount, "Lazy loader was accessed prematurely.");
            var testModel = testKey.LazyValue.Value;
            Assert.AreSame(_models[1], testModel, "Model does not match.");
            Assert.AreEqual(1, _testAccessCount, "Lazy loader access count is incorrect.");

            // now let's test refresh
            secondTarget.AddKey(_models[2].Key);
            secondTarget.Flush();

            Assert.AreEqual(2, _target.Query.Count(), "Unexpected key count in original collection.");
            _target.Refresh();
            Assert.AreEqual(3, _target.Query.Count(), "Refresh failed.");
            
        }
    }
}
