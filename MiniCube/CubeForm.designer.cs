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
            this.checkBoxMousePan = new System.Windows.Forms.CheckBox();
            this.buttonTop = new System.Windows.Forms.Button();
            this.buttonBottom = new System.Windows.Forms.Button();
            this.buttonLeft = new System.Windows.Forms.Button();
            this.buttonRight = new System.Windows.Forms.Button();
            this.buttonFront = new System.Windows.Forms.Button();
            this.buttonBack = new System.Windows.Forms.Button();
            this.checkBoxDynamic = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonCalibrate
            // 
            this.buttonCalibrate.Location = new System.Drawing.Point(9, 69);
            this.buttonCalibrate.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonCalibrate.Name = "buttonCalibrate";
            this.buttonCalibrate.Size = new System.Drawing.Size(266, 69);
            this.buttonCalibrate.TabIndex = 61;
            this.buttonCalibrate.Text = "Calibrate";
            this.buttonCalibrate.UseVisualStyleBackColor = true;
            this.buttonCalibrate.Click += new System.EventHandler(this.buttonCalibrate_Click);
            // 
            // buttonReconnect
            // 
            this.buttonReconnect.Location = new System.Drawing.Point(159, 9);
            this.buttonReconnect.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonReconnect.Name = "buttonReconnect";
            this.buttonReconnect.Size = new System.Drawing.Size(163, 51);
            this.buttonReconnect.TabIndex = 72;
            this.buttonReconnect.Text = "Reconnect";
            this.buttonReconnect.UseVisualStyleBackColor = true;
            this.buttonReconnect.Click += new System.EventHandler(this.buttonReconnect_Click);
            // 
            // comboBoxPorts
            // 
            this.comboBoxPorts.FormattingEnabled = true;
            this.comboBoxPorts.Location = new System.Drawing.Point(418, 9);
            this.comboBoxPorts.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.comboBoxPorts.Name = "comboBoxPorts";
            this.comboBoxPorts.Size = new System.Drawing.Size(132, 37);
            this.comboBoxPorts.TabIndex = 71;
            this.comboBoxPorts.DropDown += new System.EventHandler(this.comboBoxPorts_DropDown);
            this.comboBoxPorts.SelectedIndexChanged += new System.EventHandler(this.comboBoxPorts_SelectedIndexChanged);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(336, 20);
            this.label27.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(63, 29);
            this.label27.TabIndex = 70;
            this.label27.Text = "Port:";
            // 
            // buttonResetVirWorld
            // 
            this.buttonResetVirWorld.Location = new System.Drawing.Point(9, 152);
            this.buttonResetVirWorld.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonResetVirWorld.Name = "buttonResetVirWorld";
            this.buttonResetVirWorld.Size = new System.Drawing.Size(266, 69);
            this.buttonResetVirWorld.TabIndex = 74;
            this.buttonResetVirWorld.Text = "Reset Virtual World";
            this.buttonResetVirWorld.UseVisualStyleBackColor = true;
            this.buttonResetVirWorld.Click += new System.EventHandler(this.buttonResetVirWorld_Click);
            // 
            // buttonSetVirWorld
            // 
            this.buttonSetVirWorld.Location = new System.Drawing.Point(289, 152);
            this.buttonSetVirWorld.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonSetVirWorld.Name = "buttonSetVirWorld";
            this.buttonSetVirWorld.Size = new System.Drawing.Size(266, 69);
            this.buttonSetVirWorld.TabIndex = 75;
            this.buttonSetVirWorld.Text = "Set Virtual World";
            this.buttonSetVirWorld.UseVisualStyleBackColor = true;
            this.buttonSetVirWorld.Click += new System.EventHandler(this.buttonSetVirWorld_Click);
            // 
            // buttonCalReset
            // 
            this.buttonCalReset.Location = new System.Drawing.Point(289, 69);
            this.buttonCalReset.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonCalReset.Name = "buttonCalReset";
            this.buttonCalReset.Size = new System.Drawing.Size(266, 69);
            this.buttonCalReset.TabIndex = 76;
            this.buttonCalReset.Text = "Cal Reset";
            this.buttonCalReset.UseVisualStyleBackColor = true;
            this.buttonCalReset.Click += new System.EventHandler(this.buttonCalReset_Click);
            // 
            // checkBoxCalNum2
            // 
            this.checkBoxCalNum2.AutoSize = true;
            this.checkBoxCalNum2.Location = new System.Drawing.Point(16, 18);
            this.checkBoxCalNum2.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.checkBoxCalNum2.Name = "checkBoxCalNum2";
            this.checkBoxCalNum2.Size = new System.Drawing.Size(113, 33);
            this.checkBoxCalNum2.TabIndex = 77;
            this.checkBoxCalNum2.Text = "Cal #2";
            this.checkBoxCalNum2.UseVisualStyleBackColor = true;
            this.checkBoxCalNum2.CheckedChanged += new System.EventHandler(this.checkBoxCalNum2_CheckedChanged);
            // 
            // buttonStopServer
            // 
            this.buttonStopServer.Location = new System.Drawing.Point(9, 234);
            this.buttonStopServer.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonStopServer.Name = "buttonStopServer";
            this.buttonStopServer.Size = new System.Drawing.Size(266, 69);
            this.buttonStopServer.TabIndex = 78;
            this.buttonStopServer.Text = "Stop Server";
            this.buttonStopServer.UseVisualStyleBackColor = true;
            this.buttonStopServer.Click += new System.EventHandler(this.buttonStopServer_Click);
            // 
            // buttonStartServer
            // 
            this.buttonStartServer.Location = new System.Drawing.Point(289, 234);
            this.buttonStartServer.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonStartServer.Name = "buttonStartServer";
            this.buttonStartServer.Size = new System.Drawing.Size(266, 69);
            this.buttonStartServer.TabIndex = 79;
            this.buttonStartServer.Text = "Start Server";
            this.buttonStartServer.UseVisualStyleBackColor = true;
            this.buttonStartServer.Click += new System.EventHandler(this.buttonStartServer_Click);
            // 
            // checkBoxMousePan
            // 
            this.checkBoxMousePan.AutoSize = true;
            this.checkBoxMousePan.Location = new System.Drawing.Point(16, 317);
            this.checkBoxMousePan.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.checkBoxMousePan.Name = "checkBoxMousePan";
            this.checkBoxMousePan.Size = new System.Drawing.Size(166, 33);
            this.checkBoxMousePan.TabIndex = 80;
            this.checkBoxMousePan.Text = "Mouse Pan";
            this.checkBoxMousePan.UseVisualStyleBackColor = true;
            this.checkBoxMousePan.CheckedChanged += new System.EventHandler(this.checkBoxMousePan_CheckedChanged);
            // 
            // buttonTop
            // 
            this.buttonTop.Location = new System.Drawing.Point(744, 16);
            this.buttonTop.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonTop.Name = "buttonTop";
            this.buttonTop.Size = new System.Drawing.Size(117, 69);
            this.buttonTop.TabIndex = 81;
            this.buttonTop.Text = "Top";
            this.buttonTop.UseVisualStyleBackColor = true;
            this.buttonTop.Click += new System.EventHandler(this.buttonTop_Click);
            // 
            // buttonBottom
            // 
            this.buttonBottom.Location = new System.Drawing.Point(744, 234);
            this.buttonBottom.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonBottom.Name = "buttonBottom";
            this.buttonBottom.Size = new System.Drawing.Size(117, 69);
            this.buttonBottom.TabIndex = 82;
            this.buttonBottom.Text = "Bottom";
            this.buttonBottom.UseVisualStyleBackColor = true;
            this.buttonBottom.Click += new System.EventHandler(this.buttonBottom_Click);
            // 
            // buttonLeft
            // 
            this.buttonLeft.Location = new System.Drawing.Point(588, 132);
            this.buttonLeft.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonLeft.Name = "buttonLeft";
            this.buttonLeft.Size = new System.Drawing.Size(117, 69);
            this.buttonLeft.TabIndex = 83;
            this.buttonLeft.Text = "Left";
            this.buttonLeft.UseVisualStyleBackColor = true;
            this.buttonLeft.Click += new System.EventHandler(this.buttonLeft_Click);
            // 
            // buttonRight
            // 
            this.buttonRight.Location = new System.Drawing.Point(917, 132);
            this.buttonRight.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonRight.Name = "buttonRight";
            this.buttonRight.Size = new System.Drawing.Size(117, 69);
            this.buttonRight.TabIndex = 84;
            this.buttonRight.Text = "Right";
            this.buttonRight.UseVisualStyleBackColor = true;
            this.buttonRight.Click += new System.EventHandler(this.buttonRight_Click);
            // 
            // buttonFront
            // 
            this.buttonFront.Location = new System.Drawing.Point(786, 98);
            this.buttonFront.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonFront.Name = "buttonFront";
            this.buttonFront.Size = new System.Drawing.Size(117, 69);
            this.buttonFront.TabIndex = 85;
            this.buttonFront.Text = "Front";
            this.buttonFront.UseVisualStyleBackColor = true;
            this.buttonFront.Click += new System.EventHandler(this.buttonFront_Click);
            // 
            // buttonBack
            // 
            this.buttonBack.Location = new System.Drawing.Point(719, 152);
            this.buttonBack.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(117, 69);
            this.buttonBack.TabIndex = 86;
            this.buttonBack.Text = "Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // checkBoxDynamic
            // 
            this.checkBoxDynamic.AutoSize = true;
            this.checkBoxDynamic.Location = new System.Drawing.Point(289, 317);
            this.checkBoxDynamic.Margin = new System.Windows.Forms.Padding(7);
            this.checkBoxDynamic.Name = "checkBoxDynamic";
            this.checkBoxDynamic.Size = new System.Drawing.Size(205, 33);
            this.checkBoxDynamic.TabIndex = 87;
            this.checkBoxDynamic.Text = "Dynamic Mode";
            this.checkBoxDynamic.UseVisualStyleBackColor = true;
            // 
            // CubeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 357);
            this.Controls.Add(this.checkBoxDynamic);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.buttonFront);
            this.Controls.Add(this.buttonRight);
            this.Controls.Add(this.buttonLeft);
            this.Controls.Add(this.buttonBottom);
            this.Controls.Add(this.buttonTop);
            this.Controls.Add(this.checkBoxMousePan);
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
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
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
        private System.Windows.Forms.CheckBox checkBoxMousePan;
        private System.Windows.Forms.Button buttonTop;
        private System.Windows.Forms.Button buttonBottom;
        private System.Windows.Forms.Button buttonLeft;
        private System.Windows.Forms.Button buttonRight;
        private System.Windows.Forms.Button buttonFront;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.CheckBox checkBoxDynamic;
    }
}

