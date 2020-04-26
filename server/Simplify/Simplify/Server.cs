using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplify
{
    public class Server
    {
        public SignalArena Arena;
        public BinarySocket Socket;

        public Server(ushort ID, byte NetID)
        {
            // Create signal area
            Arena = new SignalArena(ID, NetID);

            // Create network socket
            Socket = new BinarySocket();

            // TBD: Create and link socket

            // CB - Pass messages from arena to socket
            Socket.ReceiveCB = (byte[] array) => { Arena.Update(Message.Unpack(array)); };

            // CB - Pass messages from socket to arena
            Arena.SendUpdateCB = (Message msg) => { Socket.SendAsync(msg.Pack()); };
            
        }
    }
}
