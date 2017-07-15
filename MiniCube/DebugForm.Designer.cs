namespace MiniCube
{
    partial class DebugForm
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label27 = new System.Windows.Forms.Label();
            this.breakDisplayCheckBox = new System.Windows.Forms.CheckBox();
            this.buttonGo = new System.Windows.Forms.Button();
            this.displayTextLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDisplayX = new System.Windows.Forms.TextBox();
            this.textBoxDisplayY = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxDisplayZ = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxCurrTheta = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxCurrZ = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxCurrY = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxCurrX = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.buttonSetCurrQuat = new System.Windows.Forms.Button();
            this.buttonSetCalQuat = new System.Windows.Forms.Button();
            this.textBoxCalTheta = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBoxCalZ = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxCalY = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxCalX = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.buttonSetInvCalQuat = new System.Windows.Forms.Button();
            this.textBoxInvCalTheta = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBoxInvCalZ = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.textBoxInvCalY = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.textBoxInvCalX = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.userInputCheckBox = new System.Windows.Forms.CheckBox();
            this.textBoxCamPosZ = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.textBoxCamPosY = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.textBoxCamPosX = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.textBoxCamUpZ = new System.Windows.Forms.TextBox();
            this.label28 = new System.Windows.Forms.Label();
            this.textBoxCamUpY = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.textBoxCamUpX = new System.Windows.Forms.TextBox();
            this.label30 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxDisplayTheta = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.ImeMode = System.Windows.Forms.ImeMode.KatakanaHalf;
            this.comboBox1.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.comboBox1.Location = new System.Drawing.Point(136, 4);
            this.comboBox1.MaxDropDownItems = 20;
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(40, 21);
            this.comboBox1.TabIndex = 74;
            this.comboBox1.Text = "3";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged_1);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(4, 7);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(112, 13);
            this.label27.TabIndex = 75;
            this.label27.Text = "Orientation Correction:";
            // 
            // breakDisplayCheckBox
            // 
            this.breakDisplayCheckBox.AutoSize = true;
            this.breakDisplayCheckBox.Location = new System.Drawing.Point(8, 40);
            this.breakDisplayCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.breakDisplayCheckBox.Name = "breakDisplayCheckBox";
            this.breakDisplayCheckBox.Size = new System.Drawing.Size(91, 17);
            this.breakDisplayCheckBox.TabIndex = 76;
            this.breakDisplayCheckBox.Text = "Break Display";
            this.breakDisplayCheckBox.UseVisualStyleBackColor = true;
            this.breakDisplayCheckBox.CheckedChanged += new System.EventHandler(this.breakDisplayCheckBox_CheckedChanged);
            // 
            // buttonGo
            // 
            this.buttonGo.Location = new System.Drawing.Point(205, 4);
            this.buttonGo.Margin = new System.Windows.Forms.Padding(2);
            this.buttonGo.Name = "buttonGo";
            this.buttonGo.Size = new System.Drawing.Size(101, 70);
            this.buttonGo.TabIndex = 77;
            this.buttonGo.Text = "Go";
            this.buttonGo.UseVisualStyleBackColor = true;
            this.buttonGo.Click += new System.EventHandler(this.goButton_Click);
            // 
            // displayTextLabel
            // 
            this.displayTextLabel.AutoSize = true;
            this.displayTextLabel.Location = new System.Drawing.Point(5, 582);
            this.displayTextLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.displayTextLabel.Name = "displayTextLabel";
            this.displayTextLabel.Size = new System.Drawing.Size(11, 13);
            this.displayTextLabel.TabIndex = 78;
            this.displayTextLabel.Text = "*";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(89, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 79;
            this.label1.Text = "Displaying Quat";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 80;
            this.label2.Text = "X";
            // 
            // textBoxDisplayX
            // 
            this.textBoxDisplayX.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxDisplayX.Location = new System.Drawing.Point(6, 119);
            this.textBoxDisplayX.Name = "textBoxDisplayX";
            this.textBoxDisplayX.Size = new System.Drawing.Size(47, 20);
            this.textBoxDisplayX.TabIndex = 81;
            // 
            // textBoxDisplayY
            // 
            this.textBoxDisplayY.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxDisplayY.Location = new System.Drawing.Point(59, 119);
            this.textBoxDisplayY.Name = "textBoxDisplayY";
            this.textBoxDisplayY.Size = new System.Drawing.Size(47, 20);
            this.textBoxDisplayY.TabIndex = 83;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(74, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 82;
            this.label3.Text = "Y";
            // 
            // textBoxDisplayZ
            // 
            this.textBoxDisplayZ.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxDisplayZ.Location = new System.Drawing.Point(112, 119);
            this.textBoxDisplayZ.Name = "textBoxDisplayZ";
            this.textBoxDisplayZ.Size = new System.Drawing.Size(47, 20);
            this.textBoxDisplayZ.TabIndex = 85;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(127, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 84;
            this.label5.Text = "Z";
            // 
            // textBoxCurrTheta
            // 
            this.textBoxCurrTheta.Location = new System.Drawing.Point(183, 382);
            this.textBoxCurrTheta.Name = "textBoxCurrTheta";
            this.textBoxCurrTheta.Size = new System.Drawing.Size(47, 20);
            this.textBoxCurrTheta.TabIndex = 98;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(188, 366);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 97;
            this.label4.Text = "Theta";
            // 
            // textBoxCurrZ
            // 
            this.textBoxCurrZ.Location = new System.Drawing.Point(112, 382);
            this.textBoxCurrZ.Name = "textBoxCurrZ";
            this.textBoxCurrZ.Size = new System.Drawing.Size(47, 20);
            this.textBoxCurrZ.TabIndex = 96;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(127, 366);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(14, 13);
            this.label7.TabIndex = 95;
            this.label7.Text = "Z";
            // 
            // textBoxCurrY
            // 
            this.textBoxCurrY.Location = new System.Drawing.Point(59, 382);
            this.textBoxCurrY.Name = "textBoxCurrY";
            this.textBoxCurrY.Size = new System.Drawing.Size(47, 20);
            this.textBoxCurrY.TabIndex = 94;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(74, 366);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 13);
            this.label8.TabIndex = 93;
            this.label8.Text = "Y";
            // 
            // textBoxCurrX
            // 
            this.textBoxCurrX.Location = new System.Drawing.Point(6, 382);
            this.textBoxCurrX.Name = "textBoxCurrX";
            this.textBoxCurrX.Size = new System.Drawing.Size(47, 20);
            this.textBoxCurrX.TabIndex = 92;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(21, 366);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 13);
            this.label9.TabIndex = 91;
            this.label9.Text = "X";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(115, 344);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(52, 13);
            this.label10.TabIndex = 90;
            this.label10.Text = "Curr Quat";
            // 
            // buttonSetCurrQuat
            // 
            this.buttonSetCurrQuat.Enabled = false;
            this.buttonSetCurrQuat.Location = new System.Drawing.Point(245, 380);
            this.buttonSetCurrQuat.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSetCurrQuat.Name = "buttonSetCurrQuat";
            this.buttonSetCurrQuat.Size = new System.Drawing.Size(61, 23);
            this.buttonSetCurrQuat.TabIndex = 100;
            this.buttonSetCurrQuat.Text = "Set";
            this.buttonSetCurrQuat.UseVisualStyleBackColor = true;
            this.buttonSetCurrQuat.Click += new System.EventHandler(this.buttonSetCurrQuat_Click);
            // 
            // buttonSetCalQuat
            // 
            this.buttonSetCalQuat.Location = new System.Drawing.Point(245, 460);
            this.buttonSetCalQuat.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSetCalQuat.Name = "buttonSetCalQuat";
            this.buttonSetCalQuat.Size = new System.Drawing.Size(61, 23);
            this.buttonSetCalQuat.TabIndex = 110;
            this.buttonSetCalQuat.Text = "Set";
            this.buttonSetCalQuat.UseVisualStyleBackColor = true;
            this.buttonSetCalQuat.Click += new System.EventHandler(this.buttonSetCalQuat_Click);
            // 
            // textBoxCalTheta
            // 
            this.textBoxCalTheta.Location = new System.Drawing.Point(183, 462);
            this.textBoxCalTheta.Name = "textBoxCalTheta";
            this.textBoxCalTheta.Size = new System.Drawing.Size(47, 20);
            this.textBoxCalTheta.TabIndex = 109;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(188, 446);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 108;
            this.label11.Text = "Theta";
            // 
            // textBoxCalZ
            // 
            this.textBoxCalZ.Location = new System.Drawing.Point(112, 462);
            this.textBoxCalZ.Name = "textBoxCalZ";
            this.textBoxCalZ.Size = new System.Drawing.Size(47, 20);
            this.textBoxCalZ.TabIndex = 107;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(127, 446);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(14, 13);
            this.label12.TabIndex = 106;
            this.label12.Text = "Z";
            // 
            // textBoxCalY
            // 
            this.textBoxCalY.Location = new System.Drawing.Point(59, 462);
            this.textBoxCalY.Name = "textBoxCalY";
            this.textBoxCalY.Size = new System.Drawing.Size(47, 20);
            this.textBoxCalY.TabIndex = 105;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(74, 446);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(14, 13);
            this.label13.TabIndex = 104;
            this.label13.Text = "Y";
            // 
            // textBoxCalX
            // 
            this.textBoxCalX.Location = new System.Drawing.Point(6, 462);
            this.textBoxCalX.Name = "textBoxCalX";
            this.textBoxCalX.Size = new System.Drawing.Size(47, 20);
            this.textBoxCalX.TabIndex = 103;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(21, 446);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(14, 13);
            this.label14.TabIndex = 102;
            this.label14.Text = "X";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(102, 424);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(82, 13);
            this.label15.TabIndex = 101;
            this.label15.Text = "Calibration Quat";
            // 
            // buttonSetInvCalQuat
            // 
            this.buttonSetInvCalQuat.Location = new System.Drawing.Point(245, 544);
            this.buttonSetInvCalQuat.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSetInvCalQuat.Name = "buttonSetInvCalQuat";
            this.buttonSetInvCalQuat.Size = new System.Drawing.Size(61, 23);
            this.buttonSetInvCalQuat.TabIndex = 120;
            this.buttonSetInvCalQuat.Text = "Set";
            this.buttonSetInvCalQuat.UseVisualStyleBackColor = true;
            this.buttonSetInvCalQuat.Click += new System.EventHandler(this.buttonSetInvCalQuat_Click);
            // 
            // textBoxInvCalTheta
            // 
            this.textBoxInvCalTheta.Location = new System.Drawing.Point(183, 546);
            this.textBoxInvCalTheta.Name = "textBoxInvCalTheta";
            this.textBoxInvCalTheta.Size = new System.Drawing.Size(47, 20);
            this.textBoxInvCalTheta.TabIndex = 119;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(188, 530);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(35, 13);
            this.label16.TabIndex = 118;
            this.label16.Text = "Theta";
            // 
            // textBoxInvCalZ
            // 
            this.textBoxInvCalZ.Location = new System.Drawing.Point(112, 546);
            this.textBoxInvCalZ.Name = "textBoxInvCalZ";
            this.textBoxInvCalZ.Size = new System.Drawing.Size(47, 20);
            this.textBoxInvCalZ.TabIndex = 117;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(127, 530);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(14, 13);
            this.label17.TabIndex = 116;
            this.label17.Text = "Z";
            // 
            // textBoxInvCalY
            // 
            this.textBoxInvCalY.Location = new System.Drawing.Point(59, 546);
            this.textBoxInvCalY.Name = "textBoxInvCalY";
            this.textBoxInvCalY.Size = new System.Drawing.Size(47, 20);
            this.textBoxInvCalY.TabIndex = 115;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(74, 530);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(14, 13);
            this.label18.TabIndex = 114;
            this.label18.Text = "Y";
            // 
            // textBoxInvCalX
            // 
            this.textBoxInvCalX.Location = new System.Drawing.Point(6, 546);
            this.textBoxInvCalX.Name = "textBoxInvCalX";
            this.textBoxInvCalX.Size = new System.Drawing.Size(47, 20);
            this.textBoxInvCalX.TabIndex = 113;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(21, 530);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(14, 13);
            this.label19.TabIndex = 112;
            this.label19.Text = "X";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(94, 508);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(100, 13);
            this.label20.TabIndex = 111;
            this.label20.Text = "Inv Calibration Quat";
            // 
            // userInputCheckBox
            // 
            this.userInputCheckBox.AutoSize = true;
            this.userInputCheckBox.Location = new System.Drawing.Point(109, 40);
            this.userInputCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.userInputCheckBox.Name = "userInputCheckBox";
            this.userInputCheckBox.Size = new System.Drawing.Size(75, 17);
            this.userInputCheckBox.TabIndex = 121;
            this.userInputCheckBox.Text = "User Input";
            this.userInputCheckBox.UseVisualStyleBackColor = true;
            this.userInputCheckBox.CheckedChanged += new System.EventHandler(this.userInputCheckBox_CheckedChanged);
            // 
            // textBoxCamPosZ
            // 
            this.textBoxCamPosZ.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxCamPosZ.Location = new System.Drawing.Point(148, 192);
            this.textBoxCamPosZ.Name = "textBoxCamPosZ";
            this.textBoxCamPosZ.Size = new System.Drawing.Size(47, 20);
            this.textBoxCamPosZ.TabIndex = 128;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(163, 176);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(14, 13);
            this.label22.TabIndex = 127;
            this.label22.Text = "Z";
            // 
            // textBoxCamPosY
            // 
            this.textBoxCamPosY.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxCamPosY.Location = new System.Drawing.Point(95, 192);
            this.textBoxCamPosY.Name = "textBoxCamPosY";
            this.textBoxCamPosY.Size = new System.Drawing.Size(47, 20);
            this.textBoxCamPosY.TabIndex = 126;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(110, 176);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(14, 13);
            this.label23.TabIndex = 125;
            this.label23.Text = "Y";
            // 
            // textBoxCamPosX
            // 
            this.textBoxCamPosX.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxCamPosX.Location = new System.Drawing.Point(42, 192);
            this.textBoxCamPosX.Name = "textBoxCamPosX";
            this.textBoxCamPosX.Size = new System.Drawing.Size(47, 20);
            this.textBoxCamPosX.TabIndex = 124;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(57, 176);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(14, 13);
            this.label24.TabIndex = 123;
            this.label24.Text = "X";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label25.Location = new System.Drawing.Point(100, 153);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(49, 13);
            this.label25.TabIndex = 122;
            this.label25.Text = "Cam Pos";
            // 
            // textBoxCamUpZ
            // 
            this.textBoxCamUpZ.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxCamUpZ.Location = new System.Drawing.Point(148, 272);
            this.textBoxCamUpZ.Name = "textBoxCamUpZ";
            this.textBoxCamUpZ.Size = new System.Drawing.Size(47, 20);
            this.textBoxCamUpZ.TabIndex = 137;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(163, 256);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(14, 13);
            this.label28.TabIndex = 136;
            this.label28.Text = "Z";
            // 
            // textBoxCamUpY
            // 
            this.textBoxCamUpY.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxCamUpY.Location = new System.Drawing.Point(95, 272);
            this.textBoxCamUpY.Name = "textBoxCamUpY";
            this.textBoxCamUpY.Size = new System.Drawing.Size(47, 20);
            this.textBoxCamUpY.TabIndex = 135;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(110, 256);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(14, 13);
            this.label29.TabIndex = 134;
            this.label29.Text = "Y";
            // 
            // textBoxCamUpX
            // 
            this.textBoxCamUpX.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxCamUpX.Location = new System.Drawing.Point(42, 272);
            this.textBoxCamUpX.Name = "textBoxCamUpX";
            this.textBoxCamUpX.Size = new System.Drawing.Size(47, 20);
            this.textBoxCamUpX.TabIndex = 133;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(57, 256);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(14, 13);
            this.label30.TabIndex = 132;
            this.label30.Text = "X";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label31.Location = new System.Drawing.Point(100, 233);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(45, 13);
            this.label31.TabIndex = 131;
            this.label31.Text = "Cam Up";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(188, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 88;
            this.label6.Text = "Theta";
            // 
            // textBoxDisplayTheta
            // 
            this.textBoxDisplayTheta.BackColor = System.Drawing.SystemColors.Menu;
            this.textBoxDisplayTheta.Location = new System.Drawing.Point(183, 119);
            this.textBoxDisplayTheta.Name = "textBoxDisplayTheta";
            this.textBoxDisplayTheta.Size = new System.Drawing.Size(47, 20);
            this.textBoxDisplayTheta.TabIndex = 89;
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 605);
            this.Controls.Add(this.textBoxCamUpZ);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.textBoxCamUpY);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.textBoxCamUpX);
            this.Controls.Add(this.label30);
            this.Controls.Add(this.label31);
            this.Controls.Add(this.textBoxCamPosZ);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.textBoxCamPosY);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.textBoxCamPosX);
            this.Controls.Add(this.label24);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.userInputCheckBox);
            this.Controls.Add(this.buttonSetInvCalQuat);
            this.Controls.Add(this.textBoxInvCalTheta);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.textBoxInvCalZ);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.textBoxInvCalY);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.textBoxInvCalX);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.buttonSetCalQuat);
            this.Controls.Add(this.textBoxCalTheta);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.textBoxCalZ);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.textBoxCalY);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.textBoxCalX);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.buttonSetCurrQuat);
            this.Controls.Add(this.textBoxCurrTheta);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxCurrZ);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxCurrY);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBoxCurrX);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textBoxDisplayTheta);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxDisplayZ);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxDisplayY);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxDisplayX);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.displayTextLabel);
            this.Controls.Add(this.buttonGo);
            this.Controls.Add(this.breakDisplayCheckBox);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.comboBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DebugForm";
            this.Text = "Form2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.CheckBox breakDisplayCheckBox;
        private System.Windows.Forms.Button buttonGo;
        private System.Windows.Forms.Label displayTextLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDisplayX;
        private System.Windows.Forms.TextBox textBoxDisplayY;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxDisplayZ;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxCurrTheta;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxCurrZ;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxCurrY;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxCurrX;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button buttonSetCurrQuat;
        private System.Windows.Forms.Button buttonSetCalQuat;
        private System.Windows.Forms.TextBox textBoxCalTheta;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBoxCalZ;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBoxCalY;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxCalX;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button buttonSetInvCalQuat;
        private System.Windows.Forms.TextBox textBoxInvCalTheta;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBoxInvCalZ;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textBoxInvCalY;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox textBoxInvCalX;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox userInputCheckBox;
        private System.Windows.Forms.TextBox textBoxCamPosZ;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox textBoxCamPosY;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox textBoxCamPosX;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox textBoxCamUpZ;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox textBoxCamUpY;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox textBoxCamUpX;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxDisplayTheta;
    }
}