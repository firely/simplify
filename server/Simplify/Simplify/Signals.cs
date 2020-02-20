using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Simplify
{
    public class Signals
    {

        public static List<Signal> Unpack(byte[] data)
        {
            if (data.Length == 0)
                return null;
            // Check number of signals that has been passed
            // CRC should already have been stripped away, together with the part of the message indicating total length.
            int offset = 0;
            var signals = new List<Signal>();
            while(offset < data.Length)
            {
                if (data[offset] == 0 && data[offset+1] == 0)
                {
                    signals.Add(Unpack_Config(data, ref offset));
                }
                else
                {
                    signals.Add(Unpack_Simple(data, ref offset));
                }
            }
            return signals;
        }

        private static Signal Unpack_Simple(byte[] data, ref int offset)
        {
            
            var id = (UInt16)(data[offset] * 0x100 + data[offset + 1]);
            var sub_id = data[offset + 2];
            var type = (Signal.SignalType)data[offset + 3];
            var signal = new Signal(id, sub_id, type);
            switch (type)
            {
                case Signal.SignalType.Bool:
                    signal.Data = data[offset + 4] == 1;
                    offset = offset + 5;
                    break;
                case Signal.SignalType.Int16:
                    signal.Data = System.BitConverter.ToInt16(data, offset + 4);
                    offset = offset + 6;
                    break;
                case Signal.SignalType.Real32:
                    signal.Data = System.BitConverter.ToSingle(data, offset + 4);
                    offset = offset + 8;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return signal;
        }

        private static Signal Unpack_Config(byte[] data, ref int offset)
        {

            var id = (UInt16)(data[offset + 2] * 0x100 + data[offset + 3]);
            var sub_id = data[offset + 4];
            var type = (Signal.SignalType)data[offset + 6];
            var config = (SignalInfo.SignalConfig)data[offset + 5];
            var L = data[offset + 7]; // Name length
            var name = UnicodeEncoding.Unicode.GetString(data, offset+8, L);
            offset = offset + 8 + L;
            return new Signal(id, sub_id, type, config, name);
        }

        
        public class SignalInfo
        {
            public enum SignalConfig { Cyclic = 1, In = 2, Out = 3 };

            public SignalConfig Config;
            public string Name;

            public SignalInfo(SignalConfig config, string name)
            {
                Config = config;
                Name = name;
            }
        }

        public class Signal
        {
            public enum SignalType { Bool = 1, Int16 = 21, Real32 = 41, Real64 = 81, Int64 = 82, String = 101 };

            public Signal(UInt16 id, byte sub_id, SignalType type)
            {
                Sign_id = id; Sub_id = sub_id; Type = type;
                Info = null;
            }

            public Signal(UInt16 id, byte sub_id, SignalType type, SignalInfo.SignalConfig config, string name)
            {
                Sign_id = id; Sub_id = sub_id; Type = type;
                Info = new SignalInfo(config, name);
            }

            public bool HasMoreInfo => Info != null;

            public UInt16 Sign_id;
            public byte Sub_id;
            public SignalType Type;

            public SignalInfo Info;

            public Action<Signal> Callback;

            private object data = null;
            public object Data { get => data; set => data = value; }

            override public string ToString()
            {
                var name = !this.HasMoreInfo  ? "(Unknown)" : this.Info.Name;
                return name + " - " + Enum.GetName(typeof(SignalType), this.Type);
            }
        }

        public class SignalManager
        {
            public struct CBHandle {

                public CBHandle(int id, int sub_id, Action<Signal> CB)
                {
                    this.id = id;
                    this.sub_id = id;
                    this.CB = CB;
                }

                public int id;
                public int sub_id;
                public Action<Signal> CB;
            };

            List<CBHandle> UnMatchedCBs;

            Dictionary<int, Dictionary<int, Signal>> Data;

            /// <summary>
            /// Constructor
            /// </summary>
            public SignalManager()
            {
                Data = new Dictionary<int, Dictionary<int, Signal>>();
                UnMatchedCBs = new List<CBHandle>();
            }

            /// <summary>
            /// Pass signal to SignalManager, if not present adds it to the collection. Otherwise updates the instance present.
            /// </summary>
            /// <param name="sign">Signal to update the collection with.</param>
            public Signal Update(Signal sign)
            {
                if (false == Data.ContainsKey(sign.Sub_id))
                    Data.Add(sign.Sub_id, new Dictionary<int, Signal>());
                var coll = Data[sign.Sub_id];
                if (false == coll.ContainsKey(sign.Sign_id))
                {
                    coll.Add(sign.Sign_id, sign);
                    // Check if there is a waiting CB
                    CBHandle h;
                    do
                    {
                        h = UnMatchedCBs.Find((handle) => { return sign.Sign_id == handle.id && sign.Sub_id == handle.sub_id; });
                        if (h.CB != null)
                        {
                            sign.Callback += h.CB;
                            UnMatchedCBs.Remove(h);
                        }
                    } while (h.CB != null);

                    sign.Callback?.Invoke(sign);
                    return sign;
                }

                if (sign.HasMoreInfo)
                {
                    var old = coll[sign.Sign_id];
                    coll[sign.Sign_id] = sign;
                    sign.Data = old.Data;
                    sign.Callback = old.Callback;
                    sign.Callback?.Invoke(sign);
                    return sign;
                }
                else
                {
                    coll[sign.Sign_id].Data = sign.Data;
                    coll[sign.Sign_id].Callback?.Invoke(sign);
                    return coll[sign.Sign_id];
                    
                }
            }

            /// <summary>
            /// Pass signal to SignalManager, if not present adds it to the collection. Otherwise updates the instance present.
            /// </summary>
            /// <param name="data">Byte sequence representing one or more Signals</param>
            public void Update(byte[] data)
            {   
                var signals = Unpack(data);
                foreach (var sign in signals)
                    Update(sign);
                return;
            }

            public Signal GetSignal(int id, int sub_id)
            {
                if (Data.ContainsKey(sub_id))
                    if (Data[sub_id].ContainsKey(id))
                        return Data[sub_id][id];
                return null;
            }

            public void Register(string key, Action<Signal> CB, bool persistent = true)
            {
                var keys = key.Split(':');
                if (keys.Length != 2)
                    throw new Exception("Invalid key fault");
                var A = Int32.Parse(keys[0]);
                var B = Int32.Parse(keys[1]);
                if(A < 0 || B < 0)
                    throw new Exception("Invalid key index fault");
                var sign = GetSignal(A, B);
                if (sign == null)
                {
                    UnMatchedCBs.Add(new CBHandle(B, A, CB));
                }
                else
                {
                    sign.Callback += CB;
                }
                return;
            }
        }

    }
}
