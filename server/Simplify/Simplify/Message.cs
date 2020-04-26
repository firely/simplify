using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplify
{
    public class Message
    {
        public enum eMessageType : ushort
        {
            pUPDSING = 0x1, vVERIFY = 0x2, pUPDMULT = 0x3, dDEFSING = 0x4, dDEFMULT = 0x5, dDEFRANG = 0x6, rDEFREQ = 0x7, sTIMESYNC = 0x8, rTIMEREQ = 0x9, sTIMEREP = 0x10, pEVENT = 0x11
        };

        public bool IsLittleEndian { get => BitConverter.IsLittleEndian; }

        private static uint MessageCounter = 0;

        public const ushort HEADER_SIZE = 18;

        public uint MessageID;
        public ushort TargetID;
        public byte TargetNetID;
        public ushort SourceID;
        public byte SourceNetID;
        public eMessageType MessageType;
        // public uint CheckSum;

        public byte[] Content;

        public Message()
        {
        }

        /// <summary>
        /// Builds and returns a byte[] containing a message ready for sending.
        /// </summary>
        /// <returns>Byte array containing message to be sent.</returns>
        public byte[] Pack()
        {
            Validate();

            uint length = (uint) (HEADER_SIZE + (Content != null ? Content.Length : 0)); // Calculate message length
            var data = new byte[length];

            byte[] tmp;
            int n = 0;

            tmp = BitConverter.GetBytes(length);
            data[n++] = tmp[0];
            data[n++] = tmp[1];

            if (MessageID == 0)
                MessageID = MessageCounter++;
            tmp = BitConverter.GetBytes(MessageID);
            data[n++] = tmp[0];
            data[n++] = tmp[1];
            data[n++] = tmp[2];
            data[n++] = tmp[3];

            tmp = BitConverter.GetBytes(TargetID);
            data[n++] = tmp[0];
            data[n++] = tmp[1];

            data[n++] = TargetNetID;

            tmp = BitConverter.GetBytes(SourceID);
            data[n++] = tmp[0];
            data[n++] = tmp[1];

            data[n++] = SourceNetID;

            tmp = BitConverter.GetBytes((ushort)MessageType);
            data[n++] = tmp[0];
            data[n++] = tmp[1];

            if (Content != null && Content.Length > 0)
            {
                Array.Copy(Content, 0, data, n, Content.Length);
                n = n + Content.Length;
            }

            // Checksum - not implemented
            data[n++] = 0;
            data[n++] = 0;
            data[n++] = 0;
            data[n++] = 0;

            return data;
        }

        private void Validate()
        {
            // TODO: Check that the given parameters are valid... otherwise raise a fault.
            return;
        }

        public static Message Unpack(byte[] data)
        {
            if (data == null || data.Length < 3)
                throw new Exception("Faulty data exception.");
            
            ushort length = BitConverter.ToUInt16(data, 0); // Size 2
            if (data.Length != length && length <= HEADER_SIZE)
                throw new Exception("Message wrong length exception");

            var msg = new Message();

            msg.MessageID    = BitConverter.ToUInt32(data, 2); // Size 4
            msg.TargetID     = BitConverter.ToUInt16(data, 6); // Size 2
            msg.TargetNetID  = data[8]; // Size 1
            msg.SourceID     = BitConverter.ToUInt16(data, 9); // Size 2
            msg.SourceNetID  = data[11]; // Size 1
            msg.MessageType  = (eMessageType) BitConverter.ToUInt16(data, 12); // Size 2

            msg.Content      = new byte[length - HEADER_SIZE]; // Adjustable size
            Array.Copy(data, 14, msg.Content, 0, msg.Content.Length);

            msg.Validate(); // Validate content
            return msg;
        }
    }

    public class MessageBuilder
    {
        private NodeAddress address;
        public MessageBuilder(NodeAddress address)
        {
            this.address = address;
        }

        // Generate a sTIMESYNC message, indicating the current time
        public Message sTIMESYNC()
        {
            return sTIMESYNC(DateTime.Now);
        }

        // Generate a sTIMESYNC message
        public Message sTIMESYNC(DateTime time)
        {
            return new Message()
            {
                MessageType     = Message.eMessageType.sTIMESYNC,
                SourceID        = address.ID,
                SourceNetID     = address.NetID,
                TargetID        = 0,
                TargetNetID     = 0,
                Content = BitConverter.GetBytes(time.Ticks)
            };
        }

        public Message pUPDSING(UInt32 var_id, byte data_type, byte[] raw_data, bool req_conf = false)
        {
            byte[] cont = new byte[Message.HEADER_SIZE + 13 + raw_data.Length];
            int n = 0;
            byte[] tmp = null;

            tmp = BitConverter.GetBytes(var_id);
            cont[n++] = tmp[0];
            cont[n++] = tmp[1];
            cont[n++] = tmp[2];
            cont[n++] = tmp[3];

            cont[n++] = (byte) (req_conf? 1 : 0);

            tmp = BitConverter.GetBytes(DateTime.Now.Ticks);
            cont[n++] = tmp[0];
            cont[n++] = tmp[1];
            cont[n++] = tmp[2];
            cont[n++] = tmp[3];
            cont[n++] = tmp[4];
            cont[n++] = tmp[5];
            cont[n++] = tmp[6];
            cont[n++] = tmp[7];

            cont[n++] = data_type;

            Array.Copy(raw_data, 0, cont, n++, raw_data.Length);

            return new Message()
            {
                MessageType = Message.eMessageType.pUPDSING,
                SourceID = address.ID,
                SourceNetID = address.NetID,
                TargetID = 0,
                TargetNetID = 0,
                Content = cont
            };
        }

        public Message rDEFREQ(UInt32 var_id, NodeAddress TargetNode)
        {
            return new Message()
            {
                MessageType = Message.eMessageType.rDEFREQ,
                SourceID = address.ID,
                SourceNetID = address.NetID,
                TargetID = TargetNode.ID,
                TargetNetID = TargetNode.NetID,
                Content = BitConverter.GetBytes(var_id)
            };
        }

        public Message vVERIFY(UInt32 message_id, NodeAddress TargetNode)
        {
            return new Message()
            {
                MessageType = Message.eMessageType.vVERIFY,
                SourceID = address.ID,
                SourceNetID = address.NetID,
                TargetID = TargetNode.ID,
                TargetNetID = TargetNode.NetID,
                Content = BitConverter.GetBytes(message_id)
            };
        }
    }
}
