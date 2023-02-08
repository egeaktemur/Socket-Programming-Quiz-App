namespace server
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
            this.PortBox = new System.Windows.Forms.TextBox();
            this.PortLabel = new System.Windows.Forms.Label();
            this.ServerButton = new System.Windows.Forms.Button();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.QuestionAmountLabel = new System.Windows.Forms.Label();
            this.QuestionAmountBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PortBox
            // 
            this.PortBox.Location = new System.Drawing.Point(106, 63);
            this.PortBox.Margin = new System.Windows.Forms.Padding(2);
            this.PortBox.Name = "PortBox";
            this.PortBox.Size = new System.Drawing.Size(138, 20);
            this.PortBox.TabIndex = 0;
            // 
            // PortLabel
            // 
            this.PortLabel.AutoSize = true;
            this.PortLabel.Location = new System.Drawing.Point(11, 70);
            this.PortLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.Size = new System.Drawing.Size(29, 13);
            this.PortLabel.TabIndex = 1;
            this.PortLabel.Text = "Port:";
            // 
            // ServerButton
            // 
            this.ServerButton.Location = new System.Drawing.Point(54, 157);
            this.ServerButton.Margin = new System.Windows.Forms.Padding(2);
            this.ServerButton.Name = "ServerButton";
            this.ServerButton.Size = new System.Drawing.Size(115, 22);
            this.ServerButton.TabIndex = 2;
            this.ServerButton.Text = "Open Server";
            this.ServerButton.UseVisualStyleBackColor = true;
            this.ServerButton.Click += new System.EventHandler(this.button_listen_Click);
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(263, 11);
            this.logs.Margin = new System.Windows.Forms.Padding(2);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(948, 412);
            this.logs.TabIndex = 3;
            this.logs.Text = "";
            // 
            // QuestionAmountLabel
            // 
            this.QuestionAmountLabel.Location = new System.Drawing.Point(11, 119);
            this.QuestionAmountLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.QuestionAmountLabel.Name = "QuestionAmountLabel";
            this.QuestionAmountLabel.Size = new System.Drawing.Size(91, 13);
            this.QuestionAmountLabel.TabIndex = 10;
            this.QuestionAmountLabel.Text = "Question Amount:";
            // 
            // QuestionAmountBox
            // 
            this.QuestionAmountBox.Location = new System.Drawing.Point(106, 116);
            this.QuestionAmountBox.Margin = new System.Windows.Forms.Padding(2);
            this.QuestionAmountBox.Name = "QuestionAmountBox";
            this.QuestionAmountBox.Size = new System.Drawing.Size(138, 20);
            this.QuestionAmountBox.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(72, 201);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Disconnect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1222, 434);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.QuestionAmountLabel);
            this.Controls.Add(this.QuestionAmountBox);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.ServerButton);
            this.Controls.Add(this.PortLabel);
            this.Controls.Add(this.PortBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox PortBox;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.Button ServerButton;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Label QuestionAmountLabel;
        private System.Windows.Forms.TextBox QuestionAmountBox;
        private System.Windows.Forms.Button button1;
    }
}

