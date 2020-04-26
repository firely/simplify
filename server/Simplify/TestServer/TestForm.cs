using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Simplify;

namespace TestServer
{
    public partial class TestForm : Form
    {

        private Simplify.Server server;

        public TestForm()
        {
            InitializeComponent();

            CustomInit();

            StartServer();
        }

        private void StartServer()
        {
            server = new Server(1,1);
        }

        private void CustomInit()
        {
            // Init controls
            field_server.LabelText = "Server status";
            field_server.StateText = "Running";
            field_server.Color = Color.LightSteelBlue;
            field_server.OnButtonClickCB = () => MessageBox.Show(field_server.LabelText);
            
            
        }
    }
}
