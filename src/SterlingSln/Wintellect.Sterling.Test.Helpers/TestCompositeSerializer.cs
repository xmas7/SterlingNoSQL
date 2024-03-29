using System;
using System.IO;
using Wintellect.Sterling.Serialization;

namespace Wintellect.Sterling.Test.Helpers
{    
    public class TestCompositeSerializer : BaseSerializer
    {
        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanSerialize(Type targetType)
        {
            return targetType.Equals(typeof(TestCompositeKeyClass));
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            var instance = (TestCompositeKeyClass)target;
            writer.Write(instance.Key1);
            writer.Write(instance.Key2);
            writer.Write(instance.Key3.ToByteArray());
            writer.Write(instance.Key4.ToFileTimeUtc());
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            return new TestCompositeKeyClass(
                reader.ReadInt32(),
                reader.ReadString(),
                new Guid(reader.ReadBytes(16)),
                DateTime.FromFileTimeUtc(reader.ReadInt64()).ToLocalTime());            
        }
    }
}
