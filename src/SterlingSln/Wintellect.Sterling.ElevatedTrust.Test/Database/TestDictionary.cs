﻿using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.ElevatedTrust.Test.Database
{
    [Tag("Dictionary")]
    [TestClass]
    public class TestDictionary
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            FileSystemHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(new ElevatedTrustDriver());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [TestMethod]
        public void TestNullDictionary()
        {
            var expected = TestClassWithDictionary.MakeTestClassWithDictionary();
            expected.DictionaryWithBaseClassAsValue = null;
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestClassWithDictionary>(key);
            
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNull(actual.DictionaryWithBaseClassAsValue, "Save/load failed: dictionary is not null.");            
        }

        [TestMethod]
        public void TestEmptyDictionary()
        {
            var expected = TestClassWithDictionary.MakeTestClassWithDictionary();
            expected.DictionaryWithBaseClassAsValue.Clear();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestClassWithDictionary>(key);
            
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.DictionaryWithBaseClassAsValue, "Save/load failed: dictionary not initialized.");
            Assert.AreEqual(0, actual.DictionaryWithBaseClassAsValue.Count, "Save/load failed: dictionary size mismatch.");
        }

        [TestMethod]
        public void TestDictionarySaveAndLoad()
        {
            var expected = TestClassWithDictionary.MakeTestClassWithDictionary();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestClassWithDictionary>(key);

            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.DictionaryWithBaseClassAsValue, "Save/load failed: dictionary not initialized.");
            Assert.IsNotNull(actual.DictionaryWithClassAsValue, "Save/load failed: dictionary not initialized.");
            Assert.IsNotNull(actual.DictionaryWithListAsValue, "Save/load failed: dictionary not initialized.");
            Assert.IsNotNull(actual.BaseDictionary, "Save/load failed: dictionary not initialized.");

            foreach (var v in expected.BaseDictionary)
            {
                Assert.IsTrue(actual.BaseDictionary.ContainsKey(v.Key), "Save/load failed: key not found.");
                Assert.AreEqual(expected.BaseDictionary[v.Key], actual.BaseDictionary[v.Key], "Save/load failed: key mismatch.");
            }

            foreach (var v in expected.DictionaryWithBaseClassAsValue)
            {
                Assert.IsTrue(actual.DictionaryWithBaseClassAsValue.ContainsKey(v.Key), "Save/load failed: key not found.");
                Assert.AreEqual(expected.DictionaryWithBaseClassAsValue[v.Key].Key,
                    actual.DictionaryWithBaseClassAsValue[v.Key].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.DictionaryWithBaseClassAsValue[v.Key].BaseProperty,
                    actual.DictionaryWithBaseClassAsValue[v.Key].BaseProperty, "Save/load failed: data mismatch.");
                Assert.AreEqual(expected.DictionaryWithBaseClassAsValue[v.Key].GetType(),
                    actual.DictionaryWithBaseClassAsValue[v.Key].GetType(), "Save/load failed: type mismatch.");
            }

            foreach (var v in expected.DictionaryWithClassAsValue)
            {
                Assert.IsTrue(actual.DictionaryWithClassAsValue.ContainsKey(v.Key), "Save/load failed: key not found.");
                Assert.AreEqual(expected.DictionaryWithClassAsValue[v.Key].Key,
                    actual.DictionaryWithClassAsValue[v.Key].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.DictionaryWithClassAsValue[v.Key].Data,
                    actual.DictionaryWithClassAsValue[v.Key].Data, "Save/load failed: data mismatch.");
                Assert.AreEqual(expected.DictionaryWithClassAsValue[v.Key].Date,
                    actual.DictionaryWithClassAsValue[v.Key].Date, "Save/load failed: date mismatch.");
                Assert.AreEqual(expected.DictionaryWithClassAsValue[v.Key].GetType(),
                    actual.DictionaryWithClassAsValue[v.Key].GetType(), "Save/load failed: type mismatch.");
            }

            foreach (var v in expected.DictionaryWithListAsValue)
            {
                Assert.IsTrue(actual.DictionaryWithListAsValue.ContainsKey(v.Key), "Save/load failed: key not found.");
                Assert.IsNotNull(actual.DictionaryWithListAsValue[v.Key], "Save/load failed: list not initialized.");
                Assert.AreEqual(expected.DictionaryWithListAsValue[v.Key].Count,
                    actual.DictionaryWithListAsValue[v.Key].Count, "Save/load failed: list size mismatch.");

                for (var x = 0; x < expected.DictionaryWithListAsValue[v.Key].Count; x++)
                {
                    Assert.AreEqual(expected.DictionaryWithListAsValue[v.Key][x].Key,
                        actual.DictionaryWithListAsValue[v.Key][x].Key, "Save/load failed: key mismatch.");
                    Assert.AreEqual(expected.DictionaryWithListAsValue[v.Key][x].Data,
                        actual.DictionaryWithListAsValue[v.Key][x].Data, "Save/load failed: data mismatch.");
                    Assert.AreEqual(expected.DictionaryWithListAsValue[v.Key][x].Date,
                        actual.DictionaryWithListAsValue[v.Key][x].Date, "Save/load failed: date mismatch.");
                }
            }
        }
    }
}
