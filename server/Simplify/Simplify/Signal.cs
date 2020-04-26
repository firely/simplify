using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplify
{
    public class Signal
    {
        public class DataTypeMismatchException : Exception
        {
            public DataTypeMismatchException(){}
            public DataTypeMismatchException(string message) : base(message) { }
            public DataTypeMismatchException(string message, Exception inner) : base(message, inner) { }
        }

        public enum eDataType{ Undefined = 0, BOOL = 1, INT32 = 11 };

        public struct SignalInfo { string name; string description; };

        // Signal information
        public UInt32 VariableID;
        public NodeAddress NodeAddress;
        public eDataType DataType;
        object data;

        public SignalInfo Information;

        public Action<Signal> DataChangedCB;

        public Signal(Message msg, NodeAddress nodeAddress)
        {
            throw new NotImplementedException();
        }

        public void Update(Message msg)
        {
            throw new NotImplementedException();
        }

        public void Write(bool data)
        {
            if (DataType != eDataType.BOOL)
                throw new DataTypeMismatchException();

            var callCB = this.data == null || data == (bool)this.data;
            this.data = data;
            if(callCB)
                DataChangedCB?.Invoke(this);
        }

        public void Write(Int32 data)
        {
            if (DataType != eDataType.INT32)
                throw new DataTypeMismatchException();
            this.data = data;

            var callCB = this.data == null || data == (Int32)this.data;
            this.data = data;
            if (callCB)
                DataChangedCB?.Invoke(this);
        }

        public object Read() => data;

        public static UInt32 GetVariableID(Message msg)
        {
            if(msg.MessageType == Message.eMessageType.pUPDSING || msg.MessageType == Message.eMessageType.pUPDSING)
                return BitConverter.ToUInt32(msg.Content, 0);
            throw new Exception("Call not valid for this message type");
        }
    }
}
