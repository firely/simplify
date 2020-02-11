using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Simplify
{
    public class Signals
    {

        public Signal[] Unpack(byte[] data)
        {
            if (data.Length == 0)
                return null;
            // Check number of signals that has been passed
            // CRC should already have been stripped away, together with the part of the message inidcating total length.
            int offset = 3; // Index 3 is the first 
            int msg_count = 0;
            while (offset < data.Length - 1)
            {
                offset += Signal.GetMsgSize((Signal.SignalType)data[offset]);
                msg_count++;
            }

            var arr = new Signal[msg_count];
            offset = 0;
            for (int i = 0; i < msg_count; i++)
            {
                arr[i].Sign_id = (UInt16)(data[offset] * 0x100 + data[offset + 1]);
                arr[i].Sub_id = data[offset + 2];
                arr[i].Type = (Signal.SignalType)data[offset + 3];
                switch (arr[i].Type)
                {
                    case Signal.SignalType.Bool:
                        arr[i].data_i = data[offset + 4];
                        break;
                    case Signal.SignalType.Int16:
                        arr[i].data_i = System.BitConverter.ToInt16(data, offset+4);
                        break;
                    case Signal.SignalType.Real32:
                        arr[i].data_f = System.BitConverter.ToSingle(data, offset + 4);
                        break;
                    default:
                        throw new NotImplementedException();
                }

            }
            return arr;
        }


        public struct Signal
        {
            public enum SignalConfig { Cyclic = 1, In = 2, Out = 3 };
            public enum SignalType { Bool = 1, Int16 = 21, Real32 = 41 };

            public static int GetMsgSize(SignalType type)
            {
                var N = (int)type;
                return 4 + (N-1) / 20;
            }

            public UInt16 Sign_id;
            public byte Sub_id;
            public SignalType Type;
            public Int64 data_i;
            public double data_f;
        }

    }
}
