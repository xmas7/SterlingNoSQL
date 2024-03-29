﻿using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    /// <summary>
    ///     Test activation-related database steps
    /// </summary>
    [Tag("Database")]
    [Tag("Activation")]
    [TestClass]
    public class TestActivation
    {        
        /// <summary>
        ///     Test for a duplicate activation using different scenarios
        /// </summary>
        [TestMethod]
        public void TestDuplicateActivation()
        {
            var engine1 = new SterlingEngine();
            var engine2 = new SterlingEngine();
            
            Assert.AreSame(engine1.SterlingDatabase, engine2.SterlingDatabase, "Sterling did not return the same database.");
            
            engine1.Activate();

            var exception = false;

            try
            {
                engine2.Activate();
            }
            catch(SterlingActivationException)
            {
                exception = true;
            }

            Assert.IsTrue(exception, "Sterling did not throw an activation exception on duplicate activation.");

            engine1.Dispose();

            // this should be ok now
            engine2.Activate();

            // now we'll duplicate it again
            exception = false;

            try
            {
                engine2.Activate();
            }
            catch (SterlingActivationException)
            {
                exception = true;
            }

            Assert.IsTrue(exception, "Sterling did not throw an activation exception on duplicate activation.");

            engine2.Dispose();            
        }

        [TestMethod]
        public void TestDuplicateClass()
        {
            using (var engine = new SterlingEngine())
            {
                engine.Activate();

                // now cheat and try to make a new sterling database directly
                var database = new SterlingDatabase(SterlingFactory.GetLogger());

                var exception = false; 
                try
                {
                    database.Activate();
                }
                catch (SterlingActivationException)
                {
                    exception = true;
                }

                Assert.IsTrue(exception, "Sterling did not throw an activation exception on duplicate activation with new database class.");
            }
        }

        [TestMethod]
        public void TestActivationNotReady()
        {
            using (var engine = new SterlingEngine())
            {
                var exception = false;

                try
                {
                    engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();
                }
                catch (SterlingNotReadyException)
                {
                    exception = true;
                }

                Assert.IsTrue(exception, "Sterling did not throw a not ready exception on premature access.");

                engine.Activate();

                // this should not fail
                engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();              
            }
        }
    }
}
