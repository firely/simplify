using Simplify;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestServer
{
    public partial class Form1 : Form
    {
        NetworkSocket socket;
        Signals.SignalManager signals;


        public Form1()
        {
            InitializeComponent();
            signals = new Signals.SignalManager();

            signals.Register("1:1", this.ledControl1.Sign_Update);
            signals.Register("1:1", this.infoControl1.Sign_Update);
            
        }

        private void Start(object sender, EventArgs e)
        {
            if (socket != null)
                return;
            socket = new NetworkSocket();
            socket.Start();
        }

        private void Stop(object sender, EventArgs e)
        {
            if (socket == null)
                return;
            socket.Stop();
            socket = null;
        }

        

        private void UpdateHMI(object sender, EventArgs e)
        {
            if (socket == null)
                return;

            var buffer = socket.GetReadData();
            foreach (var item in buffer)
            {
                StringBuilder sb = new StringBuilder();

                try
                {
                    var A = Signals.Unpack(item);
                    foreach (var a in A)
                    {
                        // signals.Update(a);
                        var b = signals.Update(a);
                        //ListBox.Items.Add(b);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    ListBox.Items.Add("Unpack-Error");
                }
                    

            }
        }
    }
}
