namespace USDA_ARS.CoreClient
{
    partial class CoreTest
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
            this.btnPullDataFromAccess = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnPullDataFromAccess
            // 
            this.btnPullDataFromAccess.Location = new System.Drawing.Point(12, 94);
            this.btnPullDataFromAccess.Name = "btnPullDataFromAccess";
            this.btnPullDataFromAccess.Size = new System.Drawing.Size(124, 49);
            this.btnPullDataFromAccess.TabIndex = 0;
            this.btnPullDataFromAccess.Text = "Pull Data From Access";
            this.btnPullDataFromAccess.UseVisualStyleBackColor = true;
            this.btnPullDataFromAccess.Click += new System.EventHandler(this.btnPullDataFromAccess_Click);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(142, 6);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResult.Size = new System.Drawing.Size(3000, 500);
            this.txtResult.TabIndex = 1;
            // 
            // CoreTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 244);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnPullDataFromAccess);
            this.Name = "CoreTest";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPullDataFromAccess;
        private System.Windows.Forms.TextBox txtResult;
    }
}

