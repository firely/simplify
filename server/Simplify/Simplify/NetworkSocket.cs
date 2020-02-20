using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simplify
{
    public class NetworkSocket
    {
        TcpListener socket;
        bool ConnectionOpen = false;

        Queue<bool> stopQueue;

        Queue<byte[]> ReadBuffer;
        string ReadBufferLock = "lock";

        Thread read_thread_ref;

        public NetworkSocket()
        {
            ReadBuffer = new Queue<byte[]>();
            stopQueue = new Queue<bool>();
            socket = new TcpListener(10030);
        }

        public void Start()
        {
            if (ConnectionOpen)
                return;

            ConnectionOpen = true;
            socket.Start();
            if (socket.Pending())
                Console.WriteLine("Requests pending.");
            socket.BeginAcceptTcpClient(WaitForConnection, null);
        }

        public void Stop()
        {
            if (!ConnectionOpen)
                return;

            ConnectionOpen = false;

            if (read_thread_ref != null)
            {
                lock (stopQueue)
                {
                    stopQueue.Enqueue(true);
                }
                read_thread_ref.Join(5000);
            }
            socket.Stop();
        }

        public Queue<byte[]> GetReadData()
        {
            Queue<byte[]> ret;
            lock (this.ReadBufferLock)
            {
                ret = this.ReadBuffer;
                this.ReadBuffer = new Queue<byte[]>();

            }
            return ret;
        }

        private void WaitForConnection(IAsyncResult ar)
        {
            // Handle errors
            if (!ar.IsCompleted)
                throw new Exception("WaitForConnection fault");

            // Handle the case where WaitForConnection failed due to calling stop on object
            if (ConnectionOpen == false)
                return;

            // Create a read thread
            var client = socket.EndAcceptTcpClient(ar);
            read_thread_ref = new Thread(new ThreadStart(() => ReadThread(client)));
            read_thread_ref.Start();
            
        }

        private void ReadThread(TcpClient client)
        {
            var buffer = new byte[1024];
            var stream = client.GetStream();
            while (true)
            {
                lock (stopQueue)
                {
                    if(stopQueue.Count > 0)
                    {
                        stopQueue.Clear();
                        Console.WriteLine("ReadThread exit");
                        return;
                    }
                }

                var a = stream.Read(buffer, 0, buffer.Length);
                if (a > 0)
                {
                    var A = new byte[a-4];
                    Array.Copy(buffer, 2, A, 0, a-4);
                    lock (this.ReadBufferLock)
                    {
                        this.ReadBuffer.Enqueue(A);
                    }
                }

                Thread.Sleep(25);
            }
        }
    }
}
