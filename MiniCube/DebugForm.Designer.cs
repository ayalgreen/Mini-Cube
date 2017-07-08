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
            this.goButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
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
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30"});
            this.comboBox1.Location = new System.Drawing.Point(241, 4);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(6);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(76, 33);
            this.comboBox1.TabIndex = 74;
            this.comboBox1.Text = "8";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(7, 7);
            this.label27.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(228, 25);
            this.label27.TabIndex = 75;
            this.label27.Text = "Orientation Correction:";
            // 
            // breakDisplayCheckBox
            // 
            this.breakDisplayCheckBox.AutoSize = true;
            this.breakDisplayCheckBox.Location = new System.Drawing.Point(12, 54);
            this.breakDisplayCheckBox.Name = "breakDisplayCheckBox";
            this.breakDisplayCheckBox.Size = new System.Drawing.Size(177, 29);
            this.breakDisplayCheckBox.TabIndex = 76;
            this.breakDisplayCheckBox.Text = "Break Display";
            this.breakDisplayCheckBox.UseVisualStyleBackColor = true;
            this.breakDisplayCheckBox.CheckedChanged += new System.EventHandler(this.breakDisplayCheckBox_CheckedChanged);
            // 
            // goButton
            // 
            this.goButton.Location = new System.Drawing.Point(195, 47);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(122, 44);
            this.goButton.TabIndex = 77;
            this.goButton.Text = "Go";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 244);
            this.label1.Name = "displayTextLabel";
            this.label1.Size = new System.Drawing.Size(0, 25);
            this.label1.TabIndex = 78;
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 331);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.breakDisplayCheckBox);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.comboBox1);
            this.Name = "DebugForm";
            this.Text = "Form2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.CheckBox breakDisplayCheckBox;
        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.Label label1;
    }
}