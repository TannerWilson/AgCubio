﻿namespace View
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
            this.LogInPanel = new System.Windows.Forms.Panel();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ServerTextBox = new System.Windows.Forms.TextBox();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.FramesLabel = new System.Windows.Forms.Label();
            this.MassLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.FoodLabel = new System.Windows.Forms.Label();
            this.WidthLabel = new System.Windows.Forms.Label();
            this.LogInPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // LogInPanel
            // 
            this.LogInPanel.Controls.Add(this.ServerLabel);
            this.LogInPanel.Controls.Add(this.label1);
            this.LogInPanel.Controls.Add(this.ServerTextBox);
            this.LogInPanel.Controls.Add(this.NameTextBox);
            this.LogInPanel.Location = new System.Drawing.Point(12, 12);
            this.LogInPanel.Name = "LogInPanel";
            this.LogInPanel.Size = new System.Drawing.Size(670, 559);
            this.LogInPanel.TabIndex = 0;
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Location = new System.Drawing.Point(126, 262);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(38, 13);
            this.ServerLabel.TabIndex = 3;
            this.ServerLabel.Text = "Sever ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(126, 159);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Player Name";
            // 
            // ServerTextBox
            // 
            this.ServerTextBox.Location = new System.Drawing.Point(228, 262);
            this.ServerTextBox.Name = "ServerTextBox";
            this.ServerTextBox.Size = new System.Drawing.Size(151, 20);
            this.ServerTextBox.TabIndex = 1;
            this.ServerTextBox.Text = "localhost";
            this.ServerTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ServerTextBox_KeyDown);
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(228, 159);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(151, 20);
            this.NameTextBox.TabIndex = 0;
            this.NameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NameTextBox_KeyDown);
            // 
            // FramesLabel
            // 
            this.FramesLabel.AutoSize = true;
            this.FramesLabel.Location = new System.Drawing.Point(713, 29);
            this.FramesLabel.Name = "FramesLabel";
            this.FramesLabel.Size = new System.Drawing.Size(33, 13);
            this.FramesLabel.TabIndex = 1;
            this.FramesLabel.Text = "FPS: ";
            // 
            // MassLabel
            // 
            this.MassLabel.AutoSize = true;
            this.MassLabel.Location = new System.Drawing.Point(711, 70);
            this.MassLabel.Name = "MassLabel";
            this.MassLabel.Size = new System.Drawing.Size(35, 13);
            this.MassLabel.TabIndex = 2;
            this.MassLabel.Text = "Mass:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(711, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Food:";
            // 
            // FoodLabel
            // 
            this.FoodLabel.AutoSize = true;
            this.FoodLabel.Location = new System.Drawing.Point(751, 107);
            this.FoodLabel.Name = "FoodLabel";
            this.FoodLabel.Size = new System.Drawing.Size(13, 13);
            this.FoodLabel.TabIndex = 4;
            this.FoodLabel.Text = "0";
            // 
            // WidthLabel
            // 
            this.WidthLabel.AutoSize = true;
            this.WidthLabel.Location = new System.Drawing.Point(708, 150);
            this.WidthLabel.Name = "WidthLabel";
            this.WidthLabel.Size = new System.Drawing.Size(38, 13);
            this.WidthLabel.TabIndex = 6;
            this.WidthLabel.Text = "Width:";
            // 
            // AgCubioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 583);
            this.Controls.Add(this.WidthLabel);
            this.Controls.Add(this.FoodLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.MassLabel);
            this.Controls.Add(this.FramesLabel);
            this.Controls.Add(this.LogInPanel);
            this.Name = "AgCubioForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.AgCubioForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AdCubioForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AdCubioForm_KeyDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AdCubioForm_MouseMove);
            this.LogInPanel.ResumeLayout(false);
            this.LogInPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel LogInPanel;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.TextBox ServerTextBox;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label FramesLabel;
        private System.Windows.Forms.Label MassLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label FoodLabel;
        private System.Windows.Forms.Label WidthLabel;
    }
}

