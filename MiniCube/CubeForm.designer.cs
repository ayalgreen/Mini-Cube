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

            //Obsolete. cube should never start an app. 
            //Test to see if we started the Inventor Application.
            //If Inventor was started by running this form then call the 
            //Quit method.
            /*if (_inventorStartedByForm && inventorRunning)
            {
                _invApp.Quit();
            }
            _invApp = null;*/
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
            this.checkBoxAllInOne = new System.Windows.Forms.CheckBox();
            this.buttonReconnectCradle = new System.Windows.Forms.Button();
            this.comboBoxPortsCradle = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCalibrate
            // 
            this.buttonCalibrate.Location = new System.Drawing.Point(4, 57);
            this.buttonCalibrate.Name = "buttonCalibrate";
            this.buttonCalibrate.Size = new System.Drawing.Size(114, 31);
            this.buttonCalibrate.TabIndex = 61;
            this.buttonCalibrate.Text = "Calibrate";
            this.buttonCalibrate.UseVisualStyleBackColor = true;
            this.buttonCalibrate.Click += new System.EventHandler(this.buttonCalibrate_Click);
            // 
            // buttonReconnect
            // 
            this.buttonReconnect.Location = new System.Drawing.Point(68, 4);
            this.buttonReconnect.Name = "buttonReconnect";
            this.buttonReconnect.Size = new System.Drawing.Size(70, 23);
            this.buttonReconnect.TabIndex = 72;
            this.buttonReconnect.Text = "Reconnect";
            this.buttonReconnect.UseVisualStyleBackColor = true;
            this.buttonReconnect.Click += new System.EventHandler(this.buttonReconnect_Click);
            // 
            // comboBoxPorts
            // 
            this.comboBoxPorts.FormattingEnabled = true;
            this.comboBoxPorts.Location = new System.Drawing.Point(179, 4);
            this.comboBoxPorts.Name = "comboBoxPorts";
            this.comboBoxPorts.Size = new System.Drawing.Size(59, 21);
            this.comboBoxPorts.TabIndex = 71;
            this.comboBoxPorts.DropDown += new System.EventHandler(this.comboBoxPorts_DropDown);
            this.comboBoxPorts.SelectedIndexChanged += new System.EventHandler(this.comboBoxPorts_SelectedIndexChanged);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(144, 9);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(29, 13);
            this.label27.TabIndex = 70;
            this.label27.Text = "Port:";
            // 
            // buttonResetVirWorld
            // 
            this.buttonResetVirWorld.Location = new System.Drawing.Point(4, 94);
            this.buttonResetVirWorld.Name = "buttonResetVirWorld";
            this.buttonResetVirWorld.Size = new System.Drawing.Size(114, 31);
            this.buttonResetVirWorld.TabIndex = 74;
            this.buttonResetVirWorld.Text = "Reset Virtual World";
            this.buttonResetVirWorld.UseVisualStyleBackColor = true;
            this.buttonResetVirWorld.Click += new System.EventHandler(this.buttonResetVirWorld_Click);
            // 
            // buttonSetVirWorld
            // 
            this.buttonSetVirWorld.Location = new System.Drawing.Point(124, 94);
            this.buttonSetVirWorld.Name = "buttonSetVirWorld";
            this.buttonSetVirWorld.Size = new System.Drawing.Size(114, 31);
            this.buttonSetVirWorld.TabIndex = 75;
            this.buttonSetVirWorld.Text = "Set Virtual World";
            this.buttonSetVirWorld.UseVisualStyleBackColor = true;
            this.buttonSetVirWorld.Click += new System.EventHandler(this.buttonSetVirWorld_Click);
            // 
            // buttonCalReset
            // 
            this.buttonCalReset.Location = new System.Drawing.Point(124, 57);
            this.buttonCalReset.Name = "buttonCalReset";
            this.buttonCalReset.Size = new System.Drawing.Size(114, 31);
            this.buttonCalReset.TabIndex = 76;
            this.buttonCalReset.Text = "Cal Reset";
            this.buttonCalReset.UseVisualStyleBackColor = true;
            this.buttonCalReset.Click += new System.EventHandler(this.buttonCalReset_Click);
            // 
            // checkBoxCalNum2
            // 
            this.checkBoxCalNum2.AutoSize = true;
            this.checkBoxCalNum2.Location = new System.Drawing.Point(7, 8);
            this.checkBoxCalNum2.Name = "checkBoxCalNum2";
            this.checkBoxCalNum2.Size = new System.Drawing.Size(57, 17);
            this.checkBoxCalNum2.TabIndex = 77;
            this.checkBoxCalNum2.Text = "Cal #2";
            this.checkBoxCalNum2.UseVisualStyleBackColor = true;
            this.checkBoxCalNum2.CheckedChanged += new System.EventHandler(this.checkBoxCalNum2_CheckedChanged);
            // 
            // buttonStopServer
            // 
            this.buttonStopServer.Location = new System.Drawing.Point(4, 131);
            this.buttonStopServer.Name = "buttonStopServer";
            this.buttonStopServer.Size = new System.Drawing.Size(114, 31);
            this.buttonStopServer.TabIndex = 78;
            this.buttonStopServer.Text = "Stop Server";
            this.buttonStopServer.UseVisualStyleBackColor = true;
            this.buttonStopServer.Click += new System.EventHandler(this.buttonStopServer_Click);
            // 
            // buttonStartServer
            // 
            this.buttonStartServer.Location = new System.Drawing.Point(124, 131);
            this.buttonStartServer.Name = "buttonStartServer";
            this.buttonStartServer.Size = new System.Drawing.Size(114, 31);
            this.buttonStartServer.TabIndex = 79;
            this.buttonStartServer.Text = "Start Server";
            this.buttonStartServer.UseVisualStyleBackColor = true;
            this.buttonStartServer.Click += new System.EventHandler(this.buttonStartServer_Click);
            // 
            // checkBoxMousePan
            // 
            this.checkBoxMousePan.AutoSize = true;
            this.checkBoxMousePan.Location = new System.Drawing.Point(7, 168);
            this.checkBoxMousePan.Name = "checkBoxMousePan";
            this.checkBoxMousePan.Size = new System.Drawing.Size(80, 17);
            this.checkBoxMousePan.TabIndex = 80;
            this.checkBoxMousePan.Text = "Mouse Pan";
            this.checkBoxMousePan.UseVisualStyleBackColor = true;
            this.checkBoxMousePan.CheckedChanged += new System.EventHandler(this.checkBoxMousePan_CheckedChanged);
            // 
            // buttonTop
            // 
            this.buttonTop.Location = new System.Drawing.Point(319, 7);
            this.buttonTop.Name = "buttonTop";
            this.buttonTop.Size = new System.Drawing.Size(50, 31);
            this.buttonTop.TabIndex = 81;
            this.buttonTop.Text = "Top";
            this.buttonTop.UseVisualStyleBackColor = true;
            this.buttonTop.Click += new System.EventHandler(this.buttonTop_Click);
            // 
            // buttonBottom
            // 
            this.buttonBottom.Location = new System.Drawing.Point(319, 105);
            this.buttonBottom.Name = "buttonBottom";
            this.buttonBottom.Size = new System.Drawing.Size(50, 31);
            this.buttonBottom.TabIndex = 82;
            this.buttonBottom.Text = "Bottom";
            this.buttonBottom.UseVisualStyleBackColor = true;
            this.buttonBottom.Click += new System.EventHandler(this.buttonBottom_Click);
            // 
            // buttonLeft
            // 
            this.buttonLeft.Location = new System.Drawing.Point(252, 59);
            this.buttonLeft.Name = "buttonLeft";
            this.buttonLeft.Size = new System.Drawing.Size(50, 31);
            this.buttonLeft.TabIndex = 83;
            this.buttonLeft.Text = "Left";
            this.buttonLeft.UseVisualStyleBackColor = true;
            this.buttonLeft.Click += new System.EventHandler(this.buttonLeft_Click);
            // 
            // buttonRight
            // 
            this.buttonRight.Location = new System.Drawing.Point(393, 59);
            this.buttonRight.Name = "buttonRight";
            this.buttonRight.Size = new System.Drawing.Size(50, 31);
            this.buttonRight.TabIndex = 84;
            this.buttonRight.Text = "Right";
            this.buttonRight.UseVisualStyleBackColor = true;
            this.buttonRight.Click += new System.EventHandler(this.buttonRight_Click);
            // 
            // buttonFront
            // 
            this.buttonFront.Location = new System.Drawing.Point(337, 44);
            this.buttonFront.Name = "buttonFront";
            this.buttonFront.Size = new System.Drawing.Size(50, 31);
            this.buttonFront.TabIndex = 85;
            this.buttonFront.Text = "Front";
            this.buttonFront.UseVisualStyleBackColor = true;
            this.buttonFront.Click += new System.EventHandler(this.buttonFront_Click);
            // 
            // buttonBack
            // 
            this.buttonBack.Location = new System.Drawing.Point(308, 68);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(50, 31);
            this.buttonBack.TabIndex = 86;
            this.buttonBack.Text = "Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // checkBoxDynamic
            // 
            this.checkBoxDynamic.AutoSize = true;
            this.checkBoxDynamic.Location = new System.Drawing.Point(124, 168);
            this.checkBoxDynamic.Name = "checkBoxDynamic";
            this.checkBoxDynamic.Size = new System.Drawing.Size(97, 17);
            this.checkBoxDynamic.TabIndex = 87;
            this.checkBoxDynamic.Text = "Dynamic Mode";
            this.checkBoxDynamic.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllInOne
            // 
            this.checkBoxAllInOne.AutoSize = true;
            this.checkBoxAllInOne.Location = new System.Drawing.Point(7, 35);
            this.checkBoxAllInOne.Name = "checkBoxAllInOne";
            this.checkBoxAllInOne.Size = new System.Drawing.Size(37, 17);
            this.checkBoxAllInOne.TabIndex = 91;
            this.checkBoxAllInOne.Text = "All";
            this.checkBoxAllInOne.UseVisualStyleBackColor = true;
            this.checkBoxAllInOne.CheckedChanged += new System.EventHandler(this.checkBoxAllInOne_CheckedChanged);
            // 
            // buttonReconnectCradle
            // 
            this.buttonReconnectCradle.Location = new System.Drawing.Point(68, 31);
            this.buttonReconnectCradle.Name = "buttonReconnectCradle";
            this.buttonReconnectCradle.Size = new System.Drawing.Size(70, 23);
            this.buttonReconnectCradle.TabIndex = 90;
            this.buttonReconnectCradle.Text = "Reconnect";
            this.buttonReconnectCradle.UseVisualStyleBackColor = true;
            this.buttonReconnectCradle.Click += new System.EventHandler(this.buttonReconnectCradle_Click);
            // 
            // comboBoxPortsCradle
            // 
            this.comboBoxPortsCradle.FormattingEnabled = true;
            this.comboBoxPortsCradle.Location = new System.Drawing.Point(179, 31);
            this.comboBoxPortsCradle.Name = "comboBoxPortsCradle";
            this.comboBoxPortsCradle.Size = new System.Drawing.Size(59, 21);
            this.comboBoxPortsCradle.TabIndex = 89;
            this.comboBoxPortsCradle.SelectedIndexChanged += new System.EventHandler(this.comboBoxPortsCradle_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(144, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 88;
            this.label1.Text = "Port:";
            // 
            // CubeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 190);
            this.Controls.Add(this.checkBoxAllInOne);
            this.Controls.Add(this.buttonReconnectCradle);
            this.Controls.Add(this.comboBoxPortsCradle);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.CheckBox checkBoxAllInOne;
        private System.Windows.Forms.Button buttonReconnectCradle;
        private System.Windows.Forms.ComboBox comboBoxPortsCradle;
        private System.Windows.Forms.Label label1;
    }
}

