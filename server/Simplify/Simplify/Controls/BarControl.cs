using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simplify.Controls
{
    public partial class BarControl : UserControl
    {
        // Width constants
        int tick_w = 8;
        int text_w = 30;
        int bound_offset = 1;

        private float val = 20f;
        private float val_frac = 0.0f;
        private float minimum = 0;
        private float maximum = 100;
        [Category("0-Signal")]
        public float Minimum { get => minimum; set { minimum = value; Update_ValFrac();  this.Refresh(); } }

        [Category("0-Signal")]
        public float Maximum { get => maximum; set{ maximum = value; Update_ValFrac();  this.Refresh(); } }

        [Category("0-Signal")]
        public float Value { get => val; set { val = value; Update_ValFrac();  this.Refresh(); } }

        public BarControl() => InitializeComponent();

        public void Update_ValFrac()
        {
            if (maximum == minimum)
                val_frac = 0;
            val_frac = (val - minimum) / (maximum - minimum);
            val_frac = val_frac > 1.0f ? 1.0f : val_frac;
            val_frac = val_frac < 0.0f ? 0.0f : val_frac;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var bar_rect = new Rectangle(bound_offset, bound_offset, Size.Width - bound_offset * 2 - tick_w - text_w, Size.Height - bound_offset*2);
            var barfill_rect = new Rectangle(bound_offset, ((int)(Size.Height * (1 - val_frac))) + bound_offset*2, Size.Width - bound_offset * 2 - tick_w - text_w, ((int)(Size.Height* val_frac)) - bound_offset * 2);
            var pen_grey = new Pen(Color.Gray, 1f);

            e.Graphics.FillRectangle(Brushes.White, bar_rect);
            e.Graphics.FillRectangle(Brushes.DeepSkyBlue, barfill_rect);
            e.Graphics.DrawRectangle(pen_grey, bar_rect);
            e.Graphics.DrawLine(pen_grey, Size.Width - bound_offset * 2 - tick_w - text_w, bound_offset, Size.Width - bound_offset * 2 - text_w, bound_offset);
            e.Graphics.DrawLine(pen_grey, Size.Width - bound_offset * 2 - tick_w - text_w, Size.Height - bound_offset, Size.Width - bound_offset * 2 - text_w, Size.Height - bound_offset);
            e.Graphics.DrawString(Maximum.ToString(), Font, Brushes.Black, Size.Width - bound_offset * 2 - text_w, bound_offset);
            e.Graphics.DrawString(Minimum.ToString(), Font, Brushes.Black, Size.Width - bound_offset * 2 - text_w, Size.Height - bound_offset - 12);
            base.OnPaint(e);
        }
    }
}
