﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.ElevatedTrust.Test.Database
{
    [Tag("Query")]
    [TestClass]
    public class TestQuery
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private List<TestModel> _modelList; 

        [TestInitialize]
        public void TestInit()
        {
            FileSystemHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(new ElevatedTrustDriver());
            _modelList = new List<TestModel>();
            for (var i = 0; i < 10; i++)
            {
                _modelList.Add(TestModel.MakeTestModel());
                _databaseInstance.Save(_modelList[i]);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [TestMethod]
        public void TestSequentialQuery()
        {           
            // set up queries
            var sequential = from k in _databaseInstance.Query<TestModel, int>() orderby k.Key select k.Key;

            var idx = 0;
            foreach (var key in sequential)
            {
                Assert.AreEqual(_modelList[idx++].Key, key, "Sequential query failed: key mismatch.");
            }
        }

        [TestMethod]
        public void TestDescendingQuery()
        {             
            var descending = from k in _databaseInstance.Query<TestModel, int>() orderby k.Key descending select k.Key;
                      
            var idx = _modelList.Count - 1;
            foreach (var key in descending)
            {
                Assert.AreEqual(_modelList[idx--].Key, key, "Descending query failed: key mismatch.");
            }                   
        }

        [TestMethod]
        public void TestRangeQuery()
        {           
            var range = from k in _databaseInstance.Query<TestModel, int>()
                        where k.Key > _modelList[2].Key && k.Key < _modelList[5].Key
                        orderby k.Key
                        select k.Key;

            var idx = 3;
            foreach (var key in range)
            {
                Assert.AreEqual(_modelList[idx++].Key, key, "Range query failed: key mismatch.");
            }
        }

        [TestMethod]
        public void TestUnrolledQuery()
        {            
            _modelList.Sort((m1, m2) => m1.Data.CompareTo(m2.Data));
            var unrolled = from k in _databaseInstance.Query<TestModel, int>() orderby k.LazyValue.Value.Data select k.LazyValue.Value;

            var idx = 0;

            foreach(var model in unrolled)
            {
                Assert.AreEqual(_modelList[idx].Key, model.Key, "Unrolled query failed: key mismatch.");
                Assert.AreEqual(_modelList[idx].Date, model.Date, "Unrolled query failed: date mismatch.");
                Assert.AreEqual(_modelList[idx].Data, model.Data, "Unrolled query failed: data mismatch.");
                idx++;
            }
        }
    }
}
