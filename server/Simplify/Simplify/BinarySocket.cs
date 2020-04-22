using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Simplify
{
    class BinarySocket : IDisposable
    {
        private Socket socket;
        private Socket open_socket;

        public Action<byte[]> ReceiveCB;

        public bool Connected
        {
            get => this.socket.Connected;
        }

        /// <summary>
        /// Create a BinarySocket object
        /// </summary>
        public BinarySocket() => socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            if (socket != null)
                socket.Close();
        }

        /// <summary>
        /// Send a message asyncronously
        /// </summary>
        public void SendAsync(byte[] buffer)
        {
            if (open_socket == null)
                throw new Exception("Socket not open exception");
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(buffer, 0, buffer.Length);
            open_socket.SendAsync(e);
        }

        /// <summary>
        /// Bind socket to our IP-address and a specific port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void Bind(IPAddress address, int port) => socket.Bind(new IPEndPoint(address, port));

        /// <summary>
        /// Start Listening on port
        /// </summary>
        public void Listen()
        {
            if (socket == null)
                throw new Exception("Socket closed exception");
            if (socket.LocalEndPoint == null)
                throw new Exception("Socket not bound exception");

            socket.Listen(100);

            socket.BeginAccept(AcceptCallback, socket);
        }

        /// <summary>
        /// Internal method used to listen for connections
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState; // Get socket reference
            Socket handle;
            try
            {
                handle  = socket.EndAccept(ar); // Accept connection, receive reference to open socket
            }
            catch (ObjectDisposedException e)
            {
                return;
            }
            open_socket = handle;

            StateObject state = new StateObject();
            state.workSocket = handle;
            handle.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state); // Start receive method

            socket.BeginAccept(AcceptCallback, socket); // Start new waiting for connection.
        }

        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 262144;
            public byte[] buffer = new byte[BufferSize];
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handle = state.workSocket;

            int read = 0;

            try
            {
                read = handle.EndReceive(ar);
            }
            catch (SocketException)
            {
                read = 0;
            }catch(Exception e)
            {
                Console.WriteLine(e);
                read = 0;
            }

            if(read > 0) // Data received
            {
                var buffer_CB = new Byte[read];
                Array.Copy(state.buffer, 0, buffer_CB, 0, read);
                handle.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state); // Restart receive method
                ReceiveCB(buffer_CB);
            }
            else // No data recevied - connection has been closed
            {
                handle.Close(); // Close connection
                open_socket = null; // Remove reference
            }           
        }
    }
}
