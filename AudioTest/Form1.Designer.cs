namespace AudioTest
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
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.LabelPanel = new System.Windows.Forms.Panel();
            this.FileInfo = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.WorkPanel = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.ViewLabel = new System.Windows.Forms.Label();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button4 = new System.Windows.Forms.Button();
            this.StitchPanel = new System.Windows.Forms.Panel();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(60, 61);
            this.button1.TabIndex = 0;
            this.button1.Text = "Parse Audio";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Potential Matches found: 0";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(785, 489);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.LabelPanel);
            this.tabPage1.Controls.Add(this.FileInfo);
            this.tabPage1.Controls.Add(this.button3);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.trackBar1);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(777, 463);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Input Parsing";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // LabelPanel
            // 
            this.LabelPanel.AutoScroll = true;
            this.LabelPanel.Location = new System.Drawing.Point(211, 6);
            this.LabelPanel.Name = "LabelPanel";
            this.LabelPanel.Size = new System.Drawing.Size(561, 348);
            this.LabelPanel.TabIndex = 11;
            // 
            // FileInfo
            // 
            this.FileInfo.AutoSize = true;
            this.FileInfo.Location = new System.Drawing.Point(98, 361);
            this.FileInfo.Name = "FileInfo";
            this.FileInfo.Size = new System.Drawing.Size(44, 13);
            this.FileInfo.TabIndex = 10;
            this.FileInfo.Text = "File Info";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 326);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(86, 48);
            this.button3.TabIndex = 6;
            this.button3.Text = "Select File";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 194);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Match Threshold: 95.0%";
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 1;
            this.trackBar1.Location = new System.Drawing.Point(6, 146);
            this.trackBar1.Maximum = 1000;
            this.trackBar1.Minimum = 900;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(104, 45);
            this.trackBar1.TabIndex = 4;
            this.trackBar1.Value = 950;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.button5);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Controls.Add(this.WorkPanel);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(777, 463);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Review";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Remaining Samples:";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(39, 55);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 43);
            this.button5.TabIndex = 1;
            this.button5.Text = "Load Data";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(39, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 43);
            this.button2.TabIndex = 0;
            this.button2.Text = "Coalate Data";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // WorkPanel
            // 
            this.WorkPanel.AutoScroll = true;
            this.WorkPanel.BackColor = System.Drawing.Color.Transparent;
            this.WorkPanel.Location = new System.Drawing.Point(120, 6);
            this.WorkPanel.Name = "WorkPanel";
            this.WorkPanel.Size = new System.Drawing.Size(628, 435);
            this.WorkPanel.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.ViewLabel);
            this.tabPage3.Controls.Add(this.button8);
            this.tabPage3.Controls.Add(this.button7);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Controls.Add(this.checkBox2);
            this.tabPage3.Controls.Add(this.checkBox1);
            this.tabPage3.Controls.Add(this.button4);
            this.tabPage3.Controls.Add(this.StitchPanel);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(777, 463);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Stitching";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // ViewLabel
            // 
            this.ViewLabel.AutoSize = true;
            this.ViewLabel.Location = new System.Drawing.Point(392, 355);
            this.ViewLabel.Name = "ViewLabel";
            this.ViewLabel.Size = new System.Drawing.Size(64, 13);
            this.ViewLabel.TabIndex = 7;
            this.ViewLabel.Text = "Viewing 0/0";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(666, 339);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 44);
            this.button8.TabIndex = 6;
            this.button8.Text = "Next";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(141, 339);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 44);
            this.button7.TabIndex = 5;
            this.button7.Text = "Previous";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 202);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Waiting to Start";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(9, 122);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(82, 17);
            this.checkBox2.TabIndex = 3;
            this.checkBox2.Text = "Show Snips";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(9, 99);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(104, 17);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Show Discarded";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(25, 15);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 44);
            this.button4.TabIndex = 1;
            this.button4.Text = "Stitch Together";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // StitchPanel
            // 
            this.StitchPanel.AutoScroll = true;
            this.StitchPanel.Location = new System.Drawing.Point(119, 15);
            this.StitchPanel.Name = "StitchPanel";
            this.StitchPanel.Size = new System.Drawing.Size(653, 318);
            this.StitchPanel.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 513);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel WorkPanel;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel StitchPanel;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label FileInfo;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel LabelPanel;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Label ViewLabel;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label1;
    }
}

