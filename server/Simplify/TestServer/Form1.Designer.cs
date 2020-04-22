namespace TestServer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BtnStart = new System.Windows.Forms.Button();
            this.BtnStop = new System.Windows.Forms.Button();
            this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.ListBox = new System.Windows.Forms.ListBox();
            this.barControl2 = new Simplify.Controls.BarControl();
            this.ledControl1 = new Simplify.Controls.LedControl();
            this.infoControl1 = new Simplify.Controls.InfoControl();
            this.SuspendLayout();
            // 
            // BtnStart
            // 
            this.BtnStart.Location = new System.Drawing.Point(12, 12);
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Size = new System.Drawing.Size(75, 23);
            this.BtnStart.TabIndex = 0;
            this.BtnStart.Text = "Start";
            this.BtnStart.UseVisualStyleBackColor = true;
            this.BtnStart.Click += new System.EventHandler(this.Start);
            // 
            // BtnStop
            // 
            this.BtnStop.Location = new System.Drawing.Point(12, 41);
            this.BtnStop.Name = "BtnStop";
            this.BtnStop.Size = new System.Drawing.Size(75, 23);
            this.BtnStop.TabIndex = 1;
            this.BtnStop.Text = "Stop";
            this.BtnStop.UseVisualStyleBackColor = true;
            this.BtnStop.Click += new System.EventHandler(this.Stop);
            // 
            // UpdateTimer
            // 
            this.UpdateTimer.Enabled = true;
            this.UpdateTimer.Tick += new System.EventHandler(this.UpdateHMI);
            // 
            // ListBox
            // 
            this.ListBox.FormattingEnabled = true;
            this.ListBox.Location = new System.Drawing.Point(193, 12);
            this.ListBox.Name = "ListBox";
            this.ListBox.Size = new System.Drawing.Size(263, 368);
            this.ListBox.TabIndex = 2;
            // 
            // barControl2
            // 
            this.barControl2.BackColor = System.Drawing.Color.Transparent;
            this.barControl2.Cursor = System.Windows.Forms.Cursors.Default;
            this.barControl2.Location = new System.Drawing.Point(65, 220);
            this.barControl2.Maximum = 80F;
            this.barControl2.Minimum = -80F;
            this.barControl2.Name = "barControl2";
            this.barControl2.Size = new System.Drawing.Size(92, 172);
            this.barControl2.TabIndex = 5;
            this.barControl2.Value = 0F;
            // 
            // ledControl1
            // 
            this.ledControl1.BackColor = System.Drawing.Color.Transparent;
            this.ledControl1.Location = new System.Drawing.Point(12, 183);
            this.ledControl1.Name = "ledControl1";
            this.ledControl1.Size = new System.Drawing.Size(20, 20);
            this.ledControl1.TabIndex = 4;
            // 
            // infoControl1
            // 
            this.infoControl1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.infoControl1.Location = new System.Drawing.Point(12, 91);
            this.infoControl1.Name = "infoControl1";
            this.infoControl1.Size = new System.Drawing.Size(117, 58);
            this.infoControl1.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 437);
            this.Controls.Add(this.barControl2);
            this.Controls.Add(this.ledControl1);
            this.Controls.Add(this.infoControl1);
            this.Controls.Add(this.ListBox);
            this.Controls.Add(this.BtnStop);
            this.Controls.Add(this.BtnStart);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnStart;
        private System.Windows.Forms.Button BtnStop;
        private System.Windows.Forms.Timer UpdateTimer;
        private System.Windows.Forms.ListBox ListBox;
        private Simplify.Controls.InfoControl infoControl1;
        private Simplify.Controls.LedControl ledControl1;
        private Simplify.Controls.BarControl barControl1;
        private Simplify.Controls.BarControl barControl2;
    }
}

