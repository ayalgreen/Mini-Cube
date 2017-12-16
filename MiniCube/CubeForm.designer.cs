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
            if (_inventorStartedByForm && inventorRunning)
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
            this.buttonResetVirWorld = new System.Windows.Forms.Button();
            this.buttonSetVirWorld = new System.Windows.Forms.Button();
            this.buttonCalReset = new System.Windows.Forms.Button();
            this.checkBoxCalNum2 = new System.Windows.Forms.CheckBox();
            this.buttonStopServer = new System.Windows.Forms.Button();
            this.buttonStartServer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCalibrate
            // 
            this.buttonCalibrate.Location = new System.Drawing.Point(8, 60);
            this.buttonCalibrate.Margin = new System.Windows.Forms.Padding(6);
            this.buttonCalibrate.Name = "buttonCalibrate";
            this.buttonCalibrate.Size = new System.Drawing.Size(228, 60);
            this.buttonCalibrate.TabIndex = 61;
            this.buttonCalibrate.Text = "Calibrate";
            this.buttonCalibrate.UseVisualStyleBackColor = true;
            this.buttonCalibrate.Click += new System.EventHandler(this.buttonCalibrate_Click);
            // 
            // buttonReconnect
            // 
            this.buttonReconnect.Location = new System.Drawing.Point(136, 8);
            this.buttonReconnect.Margin = new System.Windows.Forms.Padding(6);
            this.buttonReconnect.Name = "buttonReconnect";
            this.buttonReconnect.Size = new System.Drawing.Size(140, 44);
            this.buttonReconnect.TabIndex = 72;
            this.buttonReconnect.Text = "Reconnect";
            this.buttonReconnect.UseVisualStyleBackColor = true;
            this.buttonReconnect.Click += new System.EventHandler(this.buttonReconnect_Click);
            // 
            // comboBoxPorts
            // 
            this.comboBoxPorts.FormattingEnabled = true;
            this.comboBoxPorts.Location = new System.Drawing.Point(358, 8);
            this.comboBoxPorts.Margin = new System.Windows.Forms.Padding(6);
            this.comboBoxPorts.Name = "comboBoxPorts";
            this.comboBoxPorts.Size = new System.Drawing.Size(114, 33);
            this.comboBoxPorts.TabIndex = 71;
            this.comboBoxPorts.DropDown += new System.EventHandler(this.comboBoxPorts_DropDown);
            this.comboBoxPorts.SelectedIndexChanged += new System.EventHandler(this.comboBoxPorts_SelectedIndexChanged);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(288, 17);
            this.label27.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(57, 25);
            this.label27.TabIndex = 70;
            this.label27.Text = "Port:";
            // 
            // buttonResetVirWorld
            // 
            this.buttonResetVirWorld.Location = new System.Drawing.Point(8, 131);
            this.buttonResetVirWorld.Margin = new System.Windows.Forms.Padding(6);
            this.buttonResetVirWorld.Name = "buttonResetVirWorld";
            this.buttonResetVirWorld.Size = new System.Drawing.Size(228, 60);
            this.buttonResetVirWorld.TabIndex = 74;
            this.buttonResetVirWorld.Text = "Reset Virtual World";
            this.buttonResetVirWorld.UseVisualStyleBackColor = true;
            this.buttonResetVirWorld.Click += new System.EventHandler(this.buttonResetVirWorld_Click);
            // 
            // buttonSetVirWorld
            // 
            this.buttonSetVirWorld.Location = new System.Drawing.Point(248, 131);
            this.buttonSetVirWorld.Margin = new System.Windows.Forms.Padding(6);
            this.buttonSetVirWorld.Name = "buttonSetVirWorld";
            this.buttonSetVirWorld.Size = new System.Drawing.Size(228, 60);
            this.buttonSetVirWorld.TabIndex = 75;
            this.buttonSetVirWorld.Text = "Set Virtual World";
            this.buttonSetVirWorld.UseVisualStyleBackColor = true;
            this.buttonSetVirWorld.Click += new System.EventHandler(this.buttonSetVirWorld_Click);
            // 
            // buttonCalReset
            // 
            this.buttonCalReset.Location = new System.Drawing.Point(248, 60);
            this.buttonCalReset.Margin = new System.Windows.Forms.Padding(6);
            this.buttonCalReset.Name = "buttonCalReset";
            this.buttonCalReset.Size = new System.Drawing.Size(228, 60);
            this.buttonCalReset.TabIndex = 76;
            this.buttonCalReset.Text = "Cal Reset";
            this.buttonCalReset.UseVisualStyleBackColor = true;
            this.buttonCalReset.Click += new System.EventHandler(this.buttonCalReset_Click);
            // 
            // checkBoxCalNum2
            // 
            this.checkBoxCalNum2.AutoSize = true;
            this.checkBoxCalNum2.Location = new System.Drawing.Point(14, 15);
            this.checkBoxCalNum2.Margin = new System.Windows.Forms.Padding(6);
            this.checkBoxCalNum2.Name = "checkBoxCalNum2";
            this.checkBoxCalNum2.Size = new System.Drawing.Size(106, 29);
            this.checkBoxCalNum2.TabIndex = 77;
            this.checkBoxCalNum2.Text = "Cal #2";
            this.checkBoxCalNum2.UseVisualStyleBackColor = true;
            this.checkBoxCalNum2.CheckedChanged += new System.EventHandler(this.checkBoxCalNum2_CheckedChanged);
            // 
            // buttonStopServer
            // 
            this.buttonStopServer.Location = new System.Drawing.Point(8, 202);
            this.buttonStopServer.Margin = new System.Windows.Forms.Padding(6);
            this.buttonStopServer.Name = "buttonStopServer";
            this.buttonStopServer.Size = new System.Drawing.Size(228, 60);
            this.buttonStopServer.TabIndex = 78;
            this.buttonStopServer.Text = "Stop Server";
            this.buttonStopServer.UseVisualStyleBackColor = true;
            this.buttonStopServer.Click += new System.EventHandler(this.buttonStopServer_Click);
            // 
            // buttonStartServer
            // 
            this.buttonStartServer.Location = new System.Drawing.Point(248, 202);
            this.buttonStartServer.Margin = new System.Windows.Forms.Padding(6);
            this.buttonStartServer.Name = "buttonStartServer";
            this.buttonStartServer.Size = new System.Drawing.Size(228, 60);
            this.buttonStartServer.TabIndex = 79;
            this.buttonStartServer.Text = "Start Server";
            this.buttonStartServer.UseVisualStyleBackColor = true;
            this.buttonStartServer.Click += new System.EventHandler(this.buttonStartServer_Click);
            // 
            // CubeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 273);
            this.Controls.Add(this.buttonStartServer);
            this.Controls.Add(this.buttonStopServer);
            this.Controls.Add(this.checkBoxCalNum2);
            this.Controls.Add(this.buttonCalReset);
            this.Controls.Add(this.buttonSetVirWorld);
            this.Controls.Add(this.buttonResetVirWorld);
            this.Controls.Add(this.buttonReconnect);
            this.Controls.Add(this.comboBoxPorts);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.buttonCalibrate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.Button buttonResetVirWorld;
        private System.Windows.Forms.Button buttonSetVirWorld;
        private System.Windows.Forms.Button buttonCalReset;
        private System.Windows.Forms.CheckBox checkBoxCalNum2;
        private System.Windows.Forms.Button buttonStopServer;
        public System.Windows.Forms.Button buttonStartServer;
    }
}

