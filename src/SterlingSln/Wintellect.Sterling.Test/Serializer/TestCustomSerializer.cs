using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Serialization;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Serializer
{
    /// <summary>
    ///     Example list normally not supported (IEnumerable)
    /// </summary>
    public class NotSupportedList : IEnumerable<TestModel>
    {
        private readonly List<TestModel> _list = new List<TestModel>();

        public void Add(IEnumerable<TestModel> newItems)
        {
            _list.AddRange(newItems);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TestModel> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    ///     Class typically not supported because the property is IEnumerable
    /// </summary>
    public class NotSupportedClass
    {
        public NotSupportedClass()
        {
            InnerList = new NotSupportedList();
        }

        public int Id { get; set; }

        public NotSupportedList InnerList { get; set; }
    }

    /// <summary>
    ///     Custom database to host the test
    /// </summary>
    public class CustomSerializerDatabase : BaseDatabaseInstance
    {        
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                            {
                                CreateTableDefinition<NotSupportedClass, int>(t=>t.Id),
                                CreateTableDefinition<TestModel,int>(t=>t.Key)
                            };
        }
    }

    /// <summary>
    ///     Serializer to create support for the non-supported property
    /// </summary>
    public class SupportSerializer : BaseSerializer
    {
        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type targetType)
        {
            // only support the "non-supported" list
            return targetType.Equals(typeof (NotSupportedList));
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            // turn it into a list and save it 
            var list = new List<TestModel>((NotSupportedList) target);

            // this takes advantage of the special save wrapper for injecting into the stream
            TestCustomSerializer.DatabaseInstance.Helper.Save(list, writer);
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            // grab it as a list - again, unwrapped from a node and returned
            var list = TestCustomSerializer.DatabaseInstance.Helper.Load<List<TestModel>>(reader);
            return new NotSupportedList {list};
        }
    }

#if SILVERLIGHT
    /// <summary>
    ///     Test for custom serialization
    /// </summary>
    [Tag("Custom")]
    [Tag("Serializer")]
#endif
    [TestClass]
    public class TestCustomSerializer
    {
        private SterlingEngine _engine;
        public static ISterlingDatabaseInstance DatabaseInstance;

        /// <summary>
        ///    Initialize the test
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.SterlingDatabase.RegisterSerializer<SupportSerializer>();            
            _engine.Activate();
            DatabaseInstance = _engine.SterlingDatabase.RegisterDatabase<CustomSerializerDatabase>();
            DatabaseInstance.Purge();
        }

        /// <summary>
        ///     Clean up when done
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            DatabaseInstance.Purge();
            _engine.Dispose();
            DatabaseInstance = null;
        }

        /// <summary>
        ///     Test the serializer by creating a typically non-supported class and processing with
        ///     the custom serializer
        /// </summary>
        [TestMethod]
        public void TestCustomSaveAndLoad()
        {
            var expectedList = new[] {TestModel.MakeTestModel(), TestModel.MakeTestModel(), TestModel.MakeTestModel()};
            var expected = new NotSupportedClass {Id = 1};
            expected.InnerList.Add(expectedList);

            var key = DatabaseInstance.Save(expected);

            // confirm the test models were saved as "foreign keys" 
            var count = DatabaseInstance.Query<TestModel, int>().Count();

            Assert.AreEqual(expectedList.Length, count, "Load failed: test models were not saved independently.");

            var actual = DatabaseInstance.Load<NotSupportedClass>(key);
            Assert.IsNotNull(actual, "Load failed: instance is null.");
            Assert.AreEqual(expected.Id, actual.Id, "Load failed: key mismatch.");

            // cast to list
            var actualList = new List<TestModel>(actual.InnerList);

            Assert.AreEqual(expectedList.Length, actualList.Count, "Load failed: mismatch in list.");

            foreach (var matchingItem in
                expectedList.Select(item => (from i in actualList where i.Key.Equals(item.Key) select i.Key).FirstOrDefault()).Where(matchingItem => matchingItem < 1))
            {
                Assert.Fail("Test failed: matching models not loaded.");
            }
        }        
    }
}