using System;
using Simplify;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class Message
    {
        Simplify.Message test_msg = new Simplify.Message()
        {
            MessageID = 0xF3FF5FFF,
            TargetID = 0xF5F5,
            TargetNetID = 0x66,
            SourceID = 0xE5E3,
            SourceNetID = 0x44,
            MessageType = Simplify.Message.eMessageType.dDEFRANG,
            Content = new byte[] { 0, 1, 2, 3, 4, 5 }
        }; // Create message

        [TestMethod]
        public void Pack_Unpack()
        {
            var start_msg = test_msg;
            var data = start_msg.Pack(); // Convert to byte array
            var end_msg = Simplify.Message.Unpack(data); // Convert back from byte array

            Assert.AreEqual(start_msg.MessageID, end_msg.MessageID,"MessageID mismatch");
            Assert.AreEqual(start_msg.TargetID, end_msg.TargetID, "TargetID mismatch");
            Assert.AreEqual(start_msg.TargetNetID, end_msg.TargetNetID, "TargetNetID mismatch");
            Assert.AreEqual(start_msg.SourceID, end_msg.SourceID, "SourceID mismatch");
            Assert.AreEqual(start_msg.SourceNetID, end_msg.SourceNetID, "SourceNetID mismatch");
            Assert.AreEqual(start_msg.MessageType, end_msg.MessageType, "MessageType mismatch");

            Assert.IsNotNull(start_msg.Content);
            Assert.IsNotNull(end_msg);
            Assert.IsNotNull(end_msg.Content);
            CollectionAssert.AreEqual(start_msg.Content, end_msg.Content);
        }

        [TestMethod]
        public void Pack()
        {
            var ref_data = new byte[]
            {
                24, 0, // Length
                0xFF, 0x5F, 0xFF, 0xF3, // MessageID
                0xF5, 0xF5, // TargetID
                0x66, // TargetNetID
                0xE3, 0xE5, // SourceID
                0x44, // SourceNetID
                (byte) Simplify.Message.eMessageType.dDEFRANG, 0, // MessageType
                0, 1, 2, 3, 4, 5, // Content
                0, 0, 0, 0 // Checksum
            };

            var test_data = test_msg.Pack();
            CollectionAssert.AreEqual(test_data, ref_data, "Resulting byte array is wrong");
            
        }
    }
}
