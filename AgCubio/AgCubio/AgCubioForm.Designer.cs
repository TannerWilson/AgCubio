namespace AgCubio
{
    partial class AgCubioForm
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
            this.ButtonPanel = new System.Windows.Forms.Panel();
            this.NameLabel = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.MassAmountLabel = new System.Windows.Forms.Label();
            this.FoodPreLabel = new System.Windows.Forms.Label();
            this.FoodAmountLabel = new System.Windows.Forms.Label();
            this.WidthPreLabel = new System.Windows.Forms.Label();
            this.WidthAmountLabel = new System.Windows.Forms.Label();
            this.FpsPreLabel = new System.Windows.Forms.Label();
            this.FPSAmountLabel = new System.Windows.Forms.Label();
            this.ButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this.textBox1);
            this.ButtonPanel.Controls.Add(this.label1);
            this.ButtonPanel.Controls.Add(this.NameTextBox);
            this.ButtonPanel.Controls.Add(this.NameLabel);
            this.ButtonPanel.Location = new System.Drawing.Point(12, 12);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Size = new System.Drawing.Size(919, 608);
            this.ButtonPanel.TabIndex = 0;
            //this.ButtonPanel.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.ButtonPanel_PreviewKeyDown);
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(149, 137);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(35, 13);
            this.NameLabel.TabIndex = 0;
            this.NameLabel.Text = "Name";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(191, 137);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(100, 20);
            this.NameTextBox.TabIndex = 1;
            this.NameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NameTextBox_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(140, 176);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Address";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(191, 173);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(712, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Mass: ";
            // 
            // MassAmountLabel
            // 
            this.MassAmountLabel.AutoSize = true;
            this.MassAmountLabel.Location = new System.Drawing.Point(763, 12);
            this.MassAmountLabel.Name = "MassAmountLabel";
            this.MassAmountLabel.Size = new System.Drawing.Size(13, 13);
            this.MassAmountLabel.TabIndex = 5;
            this.MassAmountLabel.Text = "0";
            // 
            // FoodPreLabel
            // 
            this.FoodPreLabel.AutoSize = true;
            this.FoodPreLabel.Location = new System.Drawing.Point(712, 39);
            this.FoodPreLabel.Name = "FoodPreLabel";
            this.FoodPreLabel.Size = new System.Drawing.Size(31, 13);
            this.FoodPreLabel.TabIndex = 6;
            this.FoodPreLabel.Text = "Food";
            // 
            // FoodAmountLabel
            // 
            this.FoodAmountLabel.AutoSize = true;
            this.FoodAmountLabel.Location = new System.Drawing.Point(763, 39);
            this.FoodAmountLabel.Name = "FoodAmountLabel";
            this.FoodAmountLabel.Size = new System.Drawing.Size(13, 13);
            this.FoodAmountLabel.TabIndex = 7;
            this.FoodAmountLabel.Text = "0";
            // 
            // WidthPreLabel
            // 
            this.WidthPreLabel.AutoSize = true;
            this.WidthPreLabel.Location = new System.Drawing.Point(712, 66);
            this.WidthPreLabel.Name = "WidthPreLabel";
            this.WidthPreLabel.Size = new System.Drawing.Size(35, 13);
            this.WidthPreLabel.TabIndex = 8;
            this.WidthPreLabel.Text = "Width";
            // 
            // WidthAmountLabel
            // 
            this.WidthAmountLabel.AutoSize = true;
            this.WidthAmountLabel.Location = new System.Drawing.Point(763, 66);
            this.WidthAmountLabel.Name = "WidthAmountLabel";
            this.WidthAmountLabel.Size = new System.Drawing.Size(13, 13);
            this.WidthAmountLabel.TabIndex = 9;
            this.WidthAmountLabel.Text = "0";
            // 
            // FpsPreLabel
            // 
            this.FpsPreLabel.AutoSize = true;
            this.FpsPreLabel.Location = new System.Drawing.Point(715, 92);
            this.FpsPreLabel.Name = "FpsPreLabel";
            this.FpsPreLabel.Size = new System.Drawing.Size(27, 13);
            this.FpsPreLabel.TabIndex = 10;
            this.FpsPreLabel.Text = "FPS";
            // 
            // FPSAmountLabel
            // 
            this.FPSAmountLabel.AutoSize = true;
            this.FPSAmountLabel.Location = new System.Drawing.Point(766, 91);
            this.FPSAmountLabel.Name = "FPSAmountLabel";
            this.FPSAmountLabel.Size = new System.Drawing.Size(57, 13);
            this.FPSAmountLabel.TabIndex = 11;
            this.FPSAmountLabel.Text = "Over 9000";
            // 
            // AgCubioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(943, 632);
            this.Controls.Add(this.FPSAmountLabel);
            this.Controls.Add(this.FpsPreLabel);
            this.Controls.Add(this.WidthAmountLabel);
            this.Controls.Add(this.WidthPreLabel);
            this.Controls.Add(this.FoodAmountLabel);
            this.Controls.Add(this.FoodPreLabel);
            this.Controls.Add(this.MassAmountLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ButtonPanel);
            this.Name = "AgCubioForm";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AgCubioForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AgCubioForm_KeyDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AgCubioForm_MouseMove);
            this.ButtonPanel.ResumeLayout(false);
            this.ButtonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel ButtonPanel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label MassAmountLabel;
        private System.Windows.Forms.Label FoodPreLabel;
        private System.Windows.Forms.Label FoodAmountLabel;
        private System.Windows.Forms.Label WidthPreLabel;
        private System.Windows.Forms.Label WidthAmountLabel;
        private System.Windows.Forms.Label FpsPreLabel;
        private System.Windows.Forms.Label FPSAmountLabel;
    }
}

