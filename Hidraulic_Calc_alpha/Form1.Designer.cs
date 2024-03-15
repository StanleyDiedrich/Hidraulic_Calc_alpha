namespace Hidraulic_Calc_alpha
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
            this.Start_btn = new System.Windows.Forms.Button();
            this.systemBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(74, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Выбери системы гидравлики (Подача и обратка)";
            // 
            // Start_btn
            // 
            this.Start_btn.Location = new System.Drawing.Point(137, 255);
            this.Start_btn.Name = "Start_btn";
            this.Start_btn.Size = new System.Drawing.Size(117, 31);
            this.Start_btn.TabIndex = 2;
            this.Start_btn.Text = "Поехали";
            this.Start_btn.UseVisualStyleBackColor = true;
            this.Start_btn.Click += new System.EventHandler(this.Start_btn_Click);
            // 
            // systemBox
            // 
            this.systemBox.FormattingEnabled = true;
            this.systemBox.Location = new System.Drawing.Point(38, 49);
            this.systemBox.Name = "systemBox";
            this.systemBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.systemBox.Size = new System.Drawing.Size(320, 186);
            this.systemBox.TabIndex = 3;
            this.systemBox.SelectedIndexChanged += new System.EventHandler(this.systemBox_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(389, 298);
            this.Controls.Add(this.systemBox);
            this.Controls.Add(this.Start_btn);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Start_btn;
        private System.Windows.Forms.ListBox systemBox;
    }
}