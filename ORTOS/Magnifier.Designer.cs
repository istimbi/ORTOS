namespace ORTOS
{
    partial class Magnifier
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Magnifier));
            this.Zoom = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Zoom)).BeginInit();
            this.SuspendLayout();
            // 
            // Zoom
            // 
            this.Zoom.BackColor = System.Drawing.Color.Transparent;
            this.Zoom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Zoom.Location = new System.Drawing.Point(0, 0);
            this.Zoom.Name = "Zoom";
            this.Zoom.Size = new System.Drawing.Size(223, 223);
            this.Zoom.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Zoom.TabIndex = 22;
            this.Zoom.TabStop = false;
            this.Zoom.Click += new System.EventHandler(this.Zoom_Click);
            // 
            // Magnifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 223);
            this.Controls.Add(this.Zoom);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Magnifier";
            this.Text = "Magnifier";
            this.Load += new System.EventHandler(this.Magnifier_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Zoom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Zoom;
    }
}