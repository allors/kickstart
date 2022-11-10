namespace WindowsForms
{
    partial class ProgressForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackgroundImage = global::WindowsForms.Properties.Resources.allors;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(768, 240);
            this.panel1.TabIndex = 2;
            this.panel1.UseWaitCursor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(238)))), ((int)(((byte)(227)))));
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = global::WindowsForms.Properties.Resources.spinner;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 5, 15, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(81, 69);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.UseWaitCursor = true;
            // 
            // LabelTitle
            // 
            this.LabelTitle.AutoSize = true;
            this.LabelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(89)))), ((int)(((byte)(169)))));
            this.LabelTitle.Location = new System.Drawing.Point(81, 0);
            this.LabelTitle.Margin = new System.Windows.Forms.Padding(0);
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.Size = new System.Drawing.Size(76, 36);
            this.LabelTitle.TabIndex = 0;
            this.LabelTitle.Text = "Title";
            this.LabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelTitle.UseWaitCursor = true;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(238)))), ((int)(((byte)(227)))));
            this.panel2.Controls.Add(this.LabelTitle);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 240);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(768, 69);
            this.panel2.TabIndex = 2;
            this.panel2.UseWaitCursor = true;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(768, 309);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Aviation";
            this.TopMost = true;
            this.UseWaitCursor = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LabelTitle;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel2;
    }
}