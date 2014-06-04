namespace APRSTracker
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCallsign = new System.Windows.Forms.TextBox();
            this.checkBoxRadio = new System.Windows.Forms.CheckBox();
            this.checkBoxWeb = new System.Windows.Forms.CheckBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.groupBoxCot = new System.Windows.Forms.GroupBox();
            this.textBoxCotPort = new System.Windows.Forms.TextBox();
            this.textBoxCotIp = new System.Windows.Forms.TextBox();
            this.checkBoxCot = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxLogPath = new System.Windows.Forms.TextBox();
            this.buttonSetPath = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxApiKey = new System.Windows.Forms.TextBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.panelSettings = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBoxCot.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panelSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Callsign:";
            // 
            // textBoxCallsign
            // 
            this.textBoxCallsign.Location = new System.Drawing.Point(9, 32);
            this.textBoxCallsign.Name = "textBoxCallsign";
            this.textBoxCallsign.Size = new System.Drawing.Size(100, 20);
            this.textBoxCallsign.TabIndex = 1;
            // 
            // checkBoxRadio
            // 
            this.checkBoxRadio.AutoSize = true;
            this.checkBoxRadio.Location = new System.Drawing.Point(9, 59);
            this.checkBoxRadio.Name = "checkBoxRadio";
            this.checkBoxRadio.Size = new System.Drawing.Size(76, 17);
            this.checkBoxRadio.TabIndex = 2;
            this.checkBoxRadio.Text = "Use Radio";
            this.checkBoxRadio.UseVisualStyleBackColor = true;
            // 
            // checkBoxWeb
            // 
            this.checkBoxWeb.AutoSize = true;
            this.checkBoxWeb.Location = new System.Drawing.Point(5, 167);
            this.checkBoxWeb.Name = "checkBoxWeb";
            this.checkBoxWeb.Size = new System.Drawing.Size(103, 17);
            this.checkBoxWeb.TabIndex = 3;
            this.checkBoxWeb.Text = "Use www.aprs.fi";
            this.checkBoxWeb.UseVisualStyleBackColor = true;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 496);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 4;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // groupBoxCot
            // 
            this.groupBoxCot.Controls.Add(this.textBoxCotPort);
            this.groupBoxCot.Controls.Add(this.textBoxCotIp);
            this.groupBoxCot.Controls.Add(this.checkBoxCot);
            this.groupBoxCot.Location = new System.Drawing.Point(4, 252);
            this.groupBoxCot.Name = "groupBoxCot";
            this.groupBoxCot.Size = new System.Drawing.Size(118, 95);
            this.groupBoxCot.TabIndex = 5;
            this.groupBoxCot.TabStop = false;
            this.groupBoxCot.Text = "CoT";
            // 
            // textBoxCotPort
            // 
            this.textBoxCotPort.Location = new System.Drawing.Point(7, 69);
            this.textBoxCotPort.Name = "textBoxCotPort";
            this.textBoxCotPort.Size = new System.Drawing.Size(100, 20);
            this.textBoxCotPort.TabIndex = 2;
            // 
            // textBoxCotIp
            // 
            this.textBoxCotIp.Location = new System.Drawing.Point(7, 44);
            this.textBoxCotIp.Name = "textBoxCotIp";
            this.textBoxCotIp.Size = new System.Drawing.Size(100, 20);
            this.textBoxCotIp.TabIndex = 1;
            // 
            // checkBoxCot
            // 
            this.checkBoxCot.AutoSize = true;
            this.checkBoxCot.Location = new System.Drawing.Point(7, 20);
            this.checkBoxCot.Name = "checkBoxCot";
            this.checkBoxCot.Size = new System.Drawing.Size(69, 17);
            this.checkBoxCot.TabIndex = 0;
            this.checkBoxCot.Text = "Emit CoT";
            this.checkBoxCot.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxLogPath);
            this.groupBox1.Controls.Add(this.buttonSetPath);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxApiKey);
            this.groupBox1.Controls.Add(this.textBoxCallsign);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBoxRadio);
            this.groupBox1.Controls.Add(this.checkBoxWeb);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(119, 243);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Track";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Log Folder:";
            // 
            // textBoxLogPath
            // 
            this.textBoxLogPath.Location = new System.Drawing.Point(16, 100);
            this.textBoxLogPath.Name = "textBoxLogPath";
            this.textBoxLogPath.Size = new System.Drawing.Size(100, 20);
            this.textBoxLogPath.TabIndex = 7;
            // 
            // buttonSetPath
            // 
            this.buttonSetPath.Location = new System.Drawing.Point(13, 126);
            this.buttonSetPath.Name = "buttonSetPath";
            this.buttonSetPath.Size = new System.Drawing.Size(49, 23);
            this.buttonSetPath.TabIndex = 6;
            this.buttonSetPath.Text = "Set";
            this.buttonSetPath.UseVisualStyleBackColor = true;
            this.buttonSetPath.Click += new System.EventHandler(this.buttonSetPath_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 189);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "API Key:";
            // 
            // textBoxApiKey
            // 
            this.textBoxApiKey.Location = new System.Drawing.Point(8, 205);
            this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.Size = new System.Drawing.Size(100, 20);
            this.textBoxApiKey.TabIndex = 4;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.Location = new System.Drawing.Point(155, 349);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(604, 180);
            this.textBoxOutput.TabIndex = 7;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(13, 467);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 8;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // panelSettings
            // 
            this.panelSettings.Controls.Add(this.groupBox1);
            this.panelSettings.Controls.Add(this.groupBoxCot);
            this.panelSettings.Location = new System.Drawing.Point(12, 12);
            this.panelSettings.Name = "panelSettings";
            this.panelSettings.Size = new System.Drawing.Size(132, 449);
            this.panelSettings.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(203, 283);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 531);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panelSettings);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.buttonStart);
            this.Name = "Form1";
            this.Text = "APRS Tracker";
            this.groupBoxCot.ResumeLayout(false);
            this.groupBoxCot.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panelSettings.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCallsign;
        private System.Windows.Forms.CheckBox checkBoxRadio;
        private System.Windows.Forms.CheckBox checkBoxWeb;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.GroupBox groupBoxCot;
        private System.Windows.Forms.TextBox textBoxCotPort;
        private System.Windows.Forms.TextBox textBoxCotIp;
        private System.Windows.Forms.CheckBox checkBoxCot;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Panel panelSettings;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxLogPath;
        private System.Windows.Forms.Button buttonSetPath;
    }
}

