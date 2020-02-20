using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simplify.Controls
{

    public partial class LedControl : UserControl
    {

        public void Sign_Update(Signals.Signal signal)
        {
            Console.WriteLine("Red");
            //Console.WriteLine("Update - " + this.Name);
            if (signal.Type == Signals.Signal.SignalType.Bool)
            {
                this.ColorPanel.BackColor = ((bool)signal.Data) ? Color.LawnGreen : Color.Red;
            }
        }

        public LedControl()
        {
            InitializeComponent();
        }
    }
}
