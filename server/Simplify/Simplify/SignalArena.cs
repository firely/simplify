using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplify
{
    public class SignalArena
    {
        List<NodeAddress> NodeAddresses = new List<NodeAddress>();
        List<Signal> Signals            = new List<Signal>();
        List<SignalRange> SignalRanges  = new List<SignalRange>();
        Dictionary<UInt32, Signal> SignalLookup = new Dictionary<UInt32, Signal>();

        public Action<Message> SendUpdateCB;

        private MessageBuilder messageBuilder;

        private NodeAddress address = new NodeAddress();

        public SignalArena(ushort SourceID, byte SourceNetID)
        {
            address.ID = SourceID;
            address.NetID = SourceNetID;

            messageBuilder = new MessageBuilder(address);
        }

        public void Update(Message msg)
        {
            switch (msg.MessageType){
                case Message.eMessageType.pUPDMULT:
                    throw new NotImplementedException();
                case Message.eMessageType.pUPDSING:
                    UpdateSingle(msg);
                    break;
                case Message.eMessageType.dDEFMULT:
                    throw new NotImplementedException();
                case Message.eMessageType.dDEFRANG:
                    throw new NotImplementedException();
                case Message.eMessageType.dDEFSING:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private void UpdateSingle(Message msg)
        {
            var ID = Signal.GetVariableID(msg);
            var signal = SignalLookup[Signal.GetVariableID(msg)];
            if (signal == null)
            {
                NodeAddress node = null;
                foreach (var n in NodeAddresses)
                {
                    if (n.ID == msg.SourceID && n.NetID == msg.SourceNetID)
                    {
                        node = n;
                        break;
                    }
                }
                if (node == null)
                {
                    NodeAddresses.Add(new NodeAddress() { ID = msg.SourceID, NetID = msg.SourceNetID });
                }

                // Create signal
                signal = new Signal(msg, node);
                SignalLookup.Add(ID, signal);
                Signals.Add(signal);

                // Request definition
                SendUpdateCB?.Invoke(messageBuilder.rDEFREQ(ID, node));
            }
            signal.Update(msg);

        }
    }

    public class SignalRange
    {
        ushort RangeID;
        List<Signal> signals;

        public SignalRange(ushort RangeID, IList<Signal> signals)
        {
            this.RangeID = RangeID;
            this.signals = new List<Signal>(signals);
        }

        public void Update(int raw_data_of_some_kind) 
        {
            throw new NotImplementedException();
        }
    }

    public class NodeAddress
    {
        public ushort ID;
        public byte NetID;
    }
}
