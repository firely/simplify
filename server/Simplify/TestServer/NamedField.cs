using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestServer
{
    public partial class NamedField : UserControl
    {
        public NamedField()
        {
            InitializeComponent();
        }

        // On button click CB
        public Action OnButtonClickCB;
        public string LabelText { get => label1.Text; set => label1.Text = value; }
        public string StateText { get => label2.Text; set => label2.Text = value; }
        public Color Color { get => label2.BackColor; set => label2.BackColor = value; }

        private void OnClick(object sender, EventArgs e) => OnButtonClickCB?.Invoke();
    }
}
