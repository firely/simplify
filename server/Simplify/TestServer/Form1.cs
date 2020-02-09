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
        public Form1()
        {
            InitializeComponent();
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

                foreach (var c in item)
                {
                    sb.Append(String.Format("{0,2:X} ", c));
                }
                ListBox.Items.Add(sb.ToString());
            }
        }
    }
}
