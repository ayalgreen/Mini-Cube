namespace MiniCube
{
    partial class CubeForm
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

            //Test to see if we started the Inventor Application.
            //If Inventor was started by running this form then call the 
            //Quit method.
            if (_startedByForm && inventorRunning)
            {
                _invApp.Quit();
            }
            _invApp = null;
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CubeForm));
            this.buttonCalibrate = new System.Windows.Forms.Button();
            this.buttonReconnect = new System.Windows.Forms.Button();
            this.comboBoxPorts = new System.Windows.Forms.ComboBox();
            this.label27 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.ypr2 = new System.Windows.Forms.TextBox();
            this.ypr1 = new System.Windows.Forms.TextBox();
            this.ypr0 = new System.Windows.Forms.TextBox();
            this.yprR0 = new System.Windows.Forms.TextBox();
            this.yprR1 = new System.Windows.Forms.TextBox();
            this.yprR2 = new System.Windows.Forms.TextBox();
            this.yprC0 = new System.Windows.Forms.TextBox();
            this.yprC1 = new System.Windows.Forms.TextBox();
            this.yprC2 = new System.Windows.Forms.TextBox();
            this.xR = new System.Windows.Forms.TextBox();
            this.yR = new System.Windows.Forms.TextBox();
            this.zR = new System.Windows.Forms.TextBox();
            this.xC = new System.Windows.Forms.TextBox();
            this.yC = new System.Windows.Forms.TextBox();
            this.zC = new System.Windows.Forms.TextBox();
            this.xCC = new System.Windows.Forms.TextBox();
            this.yCC = new System.Windows.Forms.TextBox();
            this.zCC = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCalibrate
            // 
            this.buttonCalibrate.Location = new System.Drawing.Point(12, 33);
            this.buttonCalibrate.Name = "buttonCalibrate";
            this.buttonCalibrate.Size = new System.Drawing.Size(199, 31);
            this.buttonCalibrate.TabIndex = 61;
            this.buttonCalibrate.Text = "Calibrate";
            this.buttonCalibrate.UseVisualStyleBackColor = true;
            this.buttonCalibrate.Click += new System.EventHandler(this.buttonCalibrate_Click);
            // 
            // buttonReconnect
            // 
            this.buttonReconnect.Location = new System.Drawing.Point(12, 4);
            this.buttonReconnect.Name = "buttonReconnect";
            this.buttonReconnect.Size = new System.Drawing.Size(99, 23);
            this.buttonReconnect.TabIndex = 72;
            this.buttonReconnect.Text = "Reconnect";
            this.buttonReconnect.UseVisualStyleBackColor = true;
            this.buttonReconnect.Click += new System.EventHandler(this.buttonReconnect_Click);
            // 
            // comboBoxPorts
            // 
            this.comboBoxPorts.FormattingEnabled = true;
            this.comboBoxPorts.Location = new System.Drawing.Point(152, 6);
            this.comboBoxPorts.Name = "comboBoxPorts";
            this.comboBoxPorts.Size = new System.Drawing.Size(59, 21);
            this.comboBoxPorts.TabIndex = 71;
            this.comboBoxPorts.SelectedIndexChanged += new System.EventHandler(this.comboBoxPorts_SelectedIndexChanged);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(117, 9);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(29, 13);
            this.label27.TabIndex = 70;
            this.label27.Text = "Port:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(199, 31);
            this.button1.TabIndex = 73;
            this.button1.Text = "Get Data";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ypr2
            // 
            this.ypr2.Location = new System.Drawing.Point(152, 122);
            this.ypr2.Name = "ypr2";
            this.ypr2.Size = new System.Drawing.Size(59, 20);
            this.ypr2.TabIndex = 74;
            // 
            // ypr1
            // 
            this.ypr1.Location = new System.Drawing.Point(79, 122);
            this.ypr1.Name = "ypr1";
            this.ypr1.Size = new System.Drawing.Size(61, 20);
            this.ypr1.TabIndex = 75;
            // 
            // ypr0
            // 
            this.ypr0.Location = new System.Drawing.Point(12, 122);
            this.ypr0.Name = "ypr0";
            this.ypr0.Size = new System.Drawing.Size(56, 20);
            this.ypr0.TabIndex = 76;
            // 
            // yprR0
            // 
            this.yprR0.Location = new System.Drawing.Point(12, 162);
            this.yprR0.Name = "yprR0";
            this.yprR0.Size = new System.Drawing.Size(56, 20);
            this.yprR0.TabIndex = 79;
            // 
            // yprR1
            // 
            this.yprR1.Location = new System.Drawing.Point(79, 162);
            this.yprR1.Name = "yprR1";
            this.yprR1.Size = new System.Drawing.Size(61, 20);
            this.yprR1.TabIndex = 78;
            // 
            // yprR2
            // 
            this.yprR2.Location = new System.Drawing.Point(152, 162);
            this.yprR2.Name = "yprR2";
            this.yprR2.Size = new System.Drawing.Size(59, 20);
            this.yprR2.TabIndex = 77;
            // 
            // yprC0
            // 
            this.yprC0.Location = new System.Drawing.Point(12, 200);
            this.yprC0.Name = "yprC0";
            this.yprC0.Size = new System.Drawing.Size(56, 20);
            this.yprC0.TabIndex = 82;
            // 
            // yprC1
            // 
            this.yprC1.Location = new System.Drawing.Point(79, 200);
            this.yprC1.Name = "yprC1";
            this.yprC1.Size = new System.Drawing.Size(61, 20);
            this.yprC1.TabIndex = 81;
            // 
            // yprC2
            // 
            this.yprC2.Location = new System.Drawing.Point(152, 200);
            this.yprC2.Name = "yprC2";
            this.yprC2.Size = new System.Drawing.Size(59, 20);
            this.yprC2.TabIndex = 80;
            // 
            // xR
            // 
            this.xR.Location = new System.Drawing.Point(12, 252);
            this.xR.Name = "xR";
            this.xR.Size = new System.Drawing.Size(56, 20);
            this.xR.TabIndex = 85;
            // 
            // yR
            // 
            this.yR.Location = new System.Drawing.Point(79, 252);
            this.yR.Name = "yR";
            this.yR.Size = new System.Drawing.Size(61, 20);
            this.yR.TabIndex = 84;
            // 
            // zR
            // 
            this.zR.Location = new System.Drawing.Point(152, 252);
            this.zR.Name = "zR";
            this.zR.Size = new System.Drawing.Size(59, 20);
            this.zR.TabIndex = 83;
            // 
            // xC
            // 
            this.xC.Location = new System.Drawing.Point(12, 285);
            this.xC.Name = "xC";
            this.xC.Size = new System.Drawing.Size(56, 20);
            this.xC.TabIndex = 88;
            // 
            // yC
            // 
            this.yC.Location = new System.Drawing.Point(79, 285);
            this.yC.Name = "yC";
            this.yC.Size = new System.Drawing.Size(61, 20);
            this.yC.TabIndex = 87;
            // 
            // zC
            // 
            this.zC.Location = new System.Drawing.Point(152, 285);
            this.zC.Name = "zC";
            this.zC.Size = new System.Drawing.Size(59, 20);
            this.zC.TabIndex = 86;
            // 
            // xCC
            // 
            this.xCC.Location = new System.Drawing.Point(12, 336);
            this.xCC.Name = "xCC";
            this.xCC.Size = new System.Drawing.Size(56, 20);
            this.xCC.TabIndex = 91;
            // 
            // yCC
            // 
            this.yCC.Location = new System.Drawing.Point(79, 336);
            this.yCC.Name = "yCC";
            this.yCC.Size = new System.Drawing.Size(61, 20);
            this.yCC.TabIndex = 90;
            // 
            // zCC
            // 
            this.zCC.Location = new System.Drawing.Point(152, 336);
            this.zCC.Name = "zCC";
            this.zCC.Size = new System.Drawing.Size(59, 20);
            this.zCC.TabIndex = 89;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 227);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 92;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 236);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 93;
            this.label2.Text = "X";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(104, 236);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 94;
            this.label3.Text = "Y";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(174, 236);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 95;
            this.label4.Text = "Z";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 106);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 96;
            this.label5.Text = "Yaw (Z)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(79, 108);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 97;
            this.label6.Text = "Pitch(Y)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(162, 108);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 98;
            this.label7.Text = "Roll(X)";
            // 
            // CubeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 391);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.xCC);
            this.Controls.Add(this.yCC);
            this.Controls.Add(this.zCC);
            this.Controls.Add(this.xC);
            this.Controls.Add(this.yC);
            this.Controls.Add(this.zC);
            this.Controls.Add(this.xR);
            this.Controls.Add(this.yR);
            this.Controls.Add(this.zR);
            this.Controls.Add(this.yprC0);
            this.Controls.Add(this.yprC1);
            this.Controls.Add(this.yprC2);
            this.Controls.Add(this.yprR0);
            this.Controls.Add(this.yprR1);
            this.Controls.Add(this.yprR2);
            this.Controls.Add(this.ypr0);
            this.Controls.Add(this.ypr1);
            this.Controls.Add(this.ypr2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonReconnect);
            this.Controls.Add(this.comboBoxPorts);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.buttonCalibrate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CubeForm";
            this.Text = "Cube";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonCalibrate;
        private System.Windows.Forms.Button buttonReconnect;
        private System.Windows.Forms.ComboBox comboBoxPorts;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox ypr2;
        private System.Windows.Forms.TextBox ypr1;
        private System.Windows.Forms.TextBox ypr0;
        private System.Windows.Forms.TextBox yprR0;
        private System.Windows.Forms.TextBox yprR1;
        private System.Windows.Forms.TextBox yprR2;
        private System.Windows.Forms.TextBox yprC0;
        private System.Windows.Forms.TextBox yprC1;
        private System.Windows.Forms.TextBox yprC2;
        private System.Windows.Forms.TextBox xR;
        private System.Windows.Forms.TextBox yR;
        private System.Windows.Forms.TextBox zR;
        private System.Windows.Forms.TextBox xC;
        private System.Windows.Forms.TextBox yC;
        private System.Windows.Forms.TextBox zC;
        private System.Windows.Forms.TextBox xCC;
        private System.Windows.Forms.TextBox yCC;
        private System.Windows.Forms.TextBox zCC;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}

