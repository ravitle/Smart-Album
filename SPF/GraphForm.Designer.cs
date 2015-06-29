namespace SPF
{
    partial class GraphForm
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
            this.components = new System.ComponentModel.Container();
            this.pboxGraph = new System.Windows.Forms.PictureBox();
            this.graphTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pboxGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // pboxGraph
            // 
            this.pboxGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pboxGraph.Location = new System.Drawing.Point(1, 1);
            this.pboxGraph.Name = "pboxGraph";
            this.pboxGraph.Size = new System.Drawing.Size(700, 700);
            this.pboxGraph.TabIndex = 1;
            this.pboxGraph.TabStop = false;
            // 
            // graphTimer
            // 
            this.graphTimer.Interval = 1000;
            this.graphTimer.Tick += new System.EventHandler(this.graphTimer_Tick);
            // 
            // GraphForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 703);
            this.Controls.Add(this.pboxGraph);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "GraphForm";
            this.Text = "GraphForm";
            this.Load += new System.EventHandler(this.GraphForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pboxGraph)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pboxGraph;
        private System.Windows.Forms.Timer graphTimer;
    }
}